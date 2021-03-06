// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Events.Applications;

namespace Reactions.Applications.Pulumi;

public static class ApplicationEvents
{
    public static async Task<IEnumerable<object>> GetEventsToAppend(this Application application, ApplicationResult applicationResult)
    {
        var events = new List<object>();

        if (application.Resources?.MongoDB?.ConnectionString is null ||
        application.Resources?.MongoDB?.ConnectionString.Value != applicationResult.MongoDB.ConnectionString)
        {
            events.Add(new MongoDBConnectionStringChangedForApplication(applicationResult.Environment, applicationResult.MongoDB.ConnectionString));
        }

        if (application.Resources?.MongoDB?.Users is null ||
            !(application.Resources?.MongoDB?.Users.Any(_ => _.UserName == "kernel") ?? false))
        {
            events.Add(new MongoDBUserChanged("kernel", applicationResult.MongoDB.Password));
        }

        var ipAddress = applicationResult.Kernel.IpAddress;
        if (application.Resources?.IpAddress is null ||
            application.Resources?.IpAddress.Value != ipAddress)
        {
            events.Add(new IpAddressSetForApplication(ipAddress));
        }

        var resourceGroupId = await applicationResult.ResourceGroup.Id.GetValue();
        if (application.Resources?.AzureResourceGroupId != resourceGroupId)
        {
            events.Add(new AzureResourceGroupCreatedForApplication(application.AzureSubscriptionId, resourceGroupId));
        }

        if (application.Resources?.AzureStorageAccountName != applicationResult.Storage.AccountName)
        {
            events.Add(new AzureStorageAccountSetForApplication(applicationResult.Storage.AccountName));
        }

        var subnets = await applicationResult.Network.VirtualNetwork.Subnets.GetValue();
        if (application.Resources?.AzureVirtualNetworkIdentifier is null ||
            application.Resources?.AzureVirtualNetworkIdentifier != subnets[0].Id!)
        {
            events.Add(new AzureVirtualNetworkIdentifierSetForApplication(subnets[0].Id!));
        }

        var networkProfile = await applicationResult.Network.Profile.Id.GetValue();
        if (application.Resources?.AzureNetworkProfileIdentifier is null ||
            application.Resources?.AzureNetworkProfileIdentifier != networkProfile)
        {
            events.Add(new AzureNetworkProfileIdentifierSetForApplication(networkProfile));
        }

        if (application.Resources?.AzureContainerRegistryLoginServer != applicationResult.ContainerRegistry.LoginServer ||
            application.Resources?.AzureContainerRegistryUserName != applicationResult.ContainerRegistry.UserName ||
            application.Resources?.AzureContainerRegistryPassword != applicationResult.ContainerRegistry.Password)
        {
            events.Add(new AzureContainerRegistrySetForApplication(
                applicationResult.ContainerRegistry.LoginServer,
                applicationResult.ContainerRegistry.UserName,
                applicationResult.ContainerRegistry.Password));
        }

        return events;
    }
}
