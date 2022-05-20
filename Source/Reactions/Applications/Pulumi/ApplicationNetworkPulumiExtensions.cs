// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.ManagedIdentity;
using Pulumi.AzureNative.Network;
using Pulumi.AzureNative.Network.Inputs;
using Pulumi.AzureNative.Resources;

namespace Reactions.Applications.Pulumi;

public static class ApplicationNetworkPulumiExtensions
{
    public static string GetPrivateZoneName(this Application application) => $"{application.Name}.local".ToLowerInvariant();

    public static NetworkResult SetupNetwork(this Application application, UserAssignedIdentity identity, Vault vault, ResourceGroup resourceGroup, Tags tags)
    {
        var virtualNetwork = new VirtualNetwork(application.Name, new()
        {
            Location = application.CloudLocation.Value,
            ResourceGroupName = resourceGroup.Name,
            Tags = tags,
            EnableDdosProtection = false,
            EnableVmProtection = false,
            AddressSpace = new AddressSpaceArgs
            {
                AddressPrefixes =
                    {
                        "10.0.0.0/16"
                    }
            },
            Subnets =
                {
                    new global::Pulumi.AzureNative.Network.Inputs.SubnetArgs
                    {
                        Name = "internal",
                        ServiceEndpoints =
                        {
                            new ServiceEndpointPropertiesFormatArgs { Service = "Microsoft.Storage" },
                            new ServiceEndpointPropertiesFormatArgs { Service = "Microsoft.KeyVault" }
                        },
                        AddressPrefix = "10.0.1.0/24",
                        PrivateEndpointNetworkPolicies = "Enabled",
                        PrivateLinkServiceNetworkPolicies = "Enabled",
                        Delegations =
                        {
                            new DelegationArgs
                            {
                                Name = "containerGroupDelegation",
                                ServiceName = "Microsoft.ContainerInstance/containerGroups"
                            }
                        }
                    },
                    new global::Pulumi.AzureNative.Network.Inputs.SubnetArgs
                    {
                        Name = "ApplicationGateway",
                        AddressPrefix = "10.0.2.0/24",
                    }
                }
        });

        var privateZoneName = application.GetPrivateZoneName();
        var privateZone = new PrivateZone("privateZone", new()
        {
            Location = "Global",
            PrivateZoneName = privateZoneName,
            ResourceGroupName = resourceGroup.Name,
            Tags = tags,
        });

        var virtualNetworkLink = new VirtualNetworkLink("virtualNetworkLink", new()
        {
            Location = "Global",
            ResourceGroupName = resourceGroup.Name,
            Tags = tags,
            RegistrationEnabled = true,
            PrivateZoneName = privateZone.Name,
            VirtualNetwork = new SubResourceArgs { Id = virtualNetwork.Id }
        });

        var profile = new NetworkProfile(application.Name, new()
        {
            Location = application.CloudLocation.Value,
            ResourceGroupName = resourceGroup.Name,
            Tags = tags,
            ContainerNetworkInterfaceConfigurations =
            {
                new ContainerNetworkInterfaceConfigurationArgs
                {
                    Name = "eth1",
                    IpConfigurations =
                    {
                        new IPConfigurationProfileArgs
                        {
                            Name = "ipconfig1",
                            Subnet = new global::Pulumi.AzureNative.Network.Inputs.SubnetArgs { Id = virtualNetwork.Subnets.Apply(_ => _[0].Id!) }
                        }
                    }
                }
            },
        });

        var publicIPAddress = new PublicIPAddress(application.Name, new()
        {
            Location = application.CloudLocation.Value,
            ResourceGroupName = resourceGroup.Name,
            Tags = tags,
            PublicIPAddressVersion = "IPv4",
            PublicIPAllocationMethod = "Static",
            PublicIpAddressName = "public",
            Sku = new PublicIPAddressSkuArgs
            {
                Name = PublicIPAddressSkuName.Standard,
                Tier = PublicIPAddressSkuTier.Regional
            }
        });

        return new(virtualNetwork, profile, privateZone, publicIPAddress);
    }
}
