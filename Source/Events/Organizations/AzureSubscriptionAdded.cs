// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.Azure;
using Concepts.Organizations;

namespace Events.Organizations
{
    [EventType("a1283443-cf53-4ef9-b6d3-d30e882da887")]
    public record AzureSubscriptionAdded(AzureSubscriptionId Id, AzureSubscriptionName Name);
}
