// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.Azure;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Resources;

namespace Reactions.Applications.Pulumi;

public static class MicroserviceContainerAppPulumiExtensions
{
    public static async Task<ContainerAppResult> SetupContainerApp(
        this Microservice microservice,
        Application application,
        ResourceGroup resourceGroup,
        AzureNetworkProfileIdentifier networkProfile,
        string managedEnvironmentId,
        string managedEnvironmentName,
        string containerRegistryLoginServer,
        string containerRegistryUsername,
        string containerRegistryPassword,
        MicroserviceStorage storage,
        IEnumerable<Deployable> deployables,
        Tags tags)
    {
        var microserviceTags = tags.Clone();
        microserviceTags["microserviceId"] = microservice.Id.ToString();
        microserviceTags["microservice"] = microservice.Name.Value;

        var storageName = $"{microservice.Name}-storage";

        var managedEnvironmentStorage = new ManagedEnvironmentsStorage(microservice.Name, new()
        {
            ResourceGroupName = resourceGroup.Name,
            EnvName = managedEnvironmentName,
            Name = storageName,
            Properties = new ManagedEnvironmentStoragePropertiesArgs
            {
                AzureFile = new AzureFilePropertiesArgs
                {
                    AccessMode = "ReadOnly",
                    AccountKey = storage.FileStorage.AccessKey,
                    AccountName = storage.FileStorage.AccountName,
                    ShareName = storage.FileStorage.ShareName
                }
            }
        });

        var containerApp = new ContainerApp(microservice.Name, new()
        {
            // Todo: We force this, due to Norway not supporting Container Apps in preview yet.
            Location = "westeurope",
            Tags = microserviceTags,
            ResourceGroupName = resourceGroup.Name,
            ManagedEnvironmentId = managedEnvironmentId,
            Configuration = new ConfigurationArgs
            {
                Ingress = new IngressArgs
                {
                    External = false,
                    TargetPort = 80,
                    AllowInsecure = true
                },
                Secrets =
                {
                    new SecretArgs()
                    {
                        Name = "container-registry",
                        Value = containerRegistryPassword
                    }
                },
                Registries =
                {
                    new RegistryCredentialsArgs()
                    {
                        Server = containerRegistryLoginServer,
                        Username = containerRegistryUsername,
                        PasswordSecretRef = "container-registry"
                    }
                }
            },
            Template = new TemplateArgs
            {
                Volumes = new VolumeArgs[]
                {
                    new()
                    {
                        Name = storageName,
                        StorageName = storageName,
                        StorageType = StorageType.AzureFile
                    }
                },
                Containers = deployables.Select(deployable => new ContainerArgs
                {
                    Name = deployable.Name.Value,
                    Image = deployable.Image,

                    VolumeMounts = new VolumeMountArgs[]
                    {
                        new()
                        {
                            MountPath = "/app/config",
                            VolumeName = storageName
                        }
                    }
                }).ToArray(),

                Scale = new ScaleArgs
                {
                    MaxReplicas = 1,
                    MinReplicas = 1,
                }
            },
        });

        var configuration = await containerApp.Configuration.GetValue();
        return new ContainerAppResult(containerApp, configuration!.Ingress!.Fqdn);
    }
}
