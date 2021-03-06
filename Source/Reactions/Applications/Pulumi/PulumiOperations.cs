// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Common;
using Concepts;
using Microsoft.Extensions.Logging;
using Pulumi.Automation;

namespace Reactions.Applications.Pulumi;

/// <summary>
/// Represents an implementation of <see cref="IPulumiOperations"/>.
/// </summary>
public class PulumiOperations : IPulumiOperations
{
    readonly ILogger<PulumiOperations> _logger;
    readonly ISettings _settings;

    public PulumiOperations(ILogger<PulumiOperations> logger, ISettings applicationSettings)
    {
        _logger = logger;
        _settings = applicationSettings;
    }

    /// <inheritdoc/>
    public void Up(Application application, string projectName, PulumiFn definition, CloudRuntimeEnvironment environment)
    {
        _ = Task.Run(async () =>
        {
            var stack = await CreateStack(application, projectName, environment, definition);
            await stack.UpAsync(new UpOptions
            {
                OnStandardOutput = Console.WriteLine,
                OnStandardError = Console.Error.WriteLine
            });
        });
    }

    /// <inheritdoc/>
    public void Down(Application application, string projectName, PulumiFn definition, CloudRuntimeEnvironment environment)
    {
        _ = Task.Run(async () =>
        {
            var stack = await CreateStack(application, projectName, environment, definition);
            await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine });
        });
    }

    /// <inheritdoc/>
    public void Remove(Application application, string projectName, PulumiFn definition, CloudRuntimeEnvironment environment)
    {
        _ = Task.Run(async () =>
        {
            var stack = await CreateStack(application, projectName, environment, definition);
            await stack.Workspace.RemoveStackAsync(environment.GetStackNameFor());
        });
    }

    /// <inheritdoc/>
    public async Task SetTag(string projectName, CloudRuntimeEnvironment environment, string tagName, string value)
    {
        var stackName = environment.GetStackNameFor();
        var payload = new
        {
            name = tagName,
            value
        };

        var url = $"https://api.pulumi.com/api/stacks/{_settings.PulumiOrganization}/{projectName}/{stackName}/tags";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.pulumi+8"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _settings.PulumiAccessToken);
        await client.PostAsJsonAsync(url, payload);
    }

    async Task<WorkspaceStack> CreateStack(Application application, string projectName, CloudRuntimeEnvironment environment, PulumiFn program)
    {
        var stackName = environment.GetStackNameFor();
        var args = new InlineProgramArgs(projectName, stackName, program)
        {
            ProjectSettings = new(projectName, ProjectRuntimeName.Dotnet),
            EnvironmentVariables = new Dictionary<string, string?>
            {
                { "TF_LOG", "TRACE" }
            }
        };
        var stack = await LocalWorkspace.CreateOrSelectStackAsync(args);

        // TODO: This should probably be hidden behind a user action with a big "Are you sure? This could leave things in an inconsistent state".
        var info = await stack.GetInfoAsync();
        if (info?.Result == UpdateState.InProgress)
        {
            await stack.CancelAsync();
        }

        await RemovePendingOperations(stack);
        await stack.Workspace.InstallPluginAsync("azure-native", "1.60.0");
        await stack.Workspace.InstallPluginAsync("mongodbatlas", "3.2.0");
        await stack.SetAllConfigAsync(new Dictionary<string, ConfigValue>
            {
                { "azure-native:location", new ConfigValue(application.CloudLocation) },
                { "azure-native:subscriptionId", new ConfigValue(application.AzureSubscriptionId.ToString()) }
            });
        var mongoDBPublicKey = _settings.MongoDBPublicKey;
        var mongoDBPrivateKey = _settings.MongoDBPrivateKey;
        Environment.SetEnvironmentVariable("MONGODB_ATLAS_PUBLIC_KEY", mongoDBPublicKey);
        Environment.SetEnvironmentVariable("MONGODB_ATLAS_PRIVATE_KEY", mongoDBPrivateKey);

        var accessToken = _settings.PulumiAccessToken;
        Environment.SetEnvironmentVariable("PULUMI_ACCESS_TOKEN", accessToken.ToString());

        await SetTag(projectName, environment, "application", application.Name);
        await SetTag(projectName, environment, "environment", stackName);

        return stack;
    }

    async Task RemovePendingOperations(WorkspaceStack stack)
    {
        _logger.ExportStack();

        var stackDeployment = await stack.ExportStackAsync();
        var jsonNode = JsonNode.Parse(stackDeployment.Json.GetRawText())!.AsObject();
        var deployment = jsonNode!["deployment"]!.AsObject();
        if (deployment["pending_operations"] is not JsonArray pendingOperations || pendingOperations.Count == 0)
        {
            _logger.NoPendingOperations();
            return;
        }

        deployment.Remove("pending_operations");
        deployment.Add("pending_operations", new JsonArray());
        var jsonString = jsonNode.ToJsonString(new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        stackDeployment = StackDeployment.FromJsonString(jsonString);

        _logger.ImportStack();
        await stack.ImportStackAsync(stackDeployment);
    }
}
