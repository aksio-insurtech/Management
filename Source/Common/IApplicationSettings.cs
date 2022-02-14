// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.MongoDB;
using Concepts.Pulumi;

namespace Common
{
    public interface IApplicationSettings
    {
        Task<PulumiAccessToken> GetPulumiAccessToken();
        Task<MongoDBOrganizationId> GetMongoDBOrganizationId();
        Task<MongoDBPublicKey> GetMongoDBPublicKey();
        Task<MongoDBPrivateKey> GetMongoDBPrivateKey();
    }
}