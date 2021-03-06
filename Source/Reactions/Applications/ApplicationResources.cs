// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.Azure;
using Concepts.Infrastructure;

namespace Reactions.Applications;

public record ApplicationResources(
    IpAddress IpAddress,
    AzureVirtualNetworkIdentifier AzureVirtualNetworkIdentifier,
    AzureNetworkProfileIdentifier AzureNetworkProfileIdentifier,
    AzureStorageAccountName AzureStorageAccountName,
    AzureContainerRegistryLoginServer AzureContainerRegistryLoginServer,
    AzureContainerRegistryUserName AzureContainerRegistryUserName,
    AzureContainerRegistryPassword AzureContainerRegistryPassword,
    AzureResourceGroupId AzureResourceGroupId,
    MongoDBResource MongoDB);
