// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.Organizations;

namespace Read.Organizations
{
    [Route("/api/organization/settings")]
    public class OrganizationSettings : Controller
    {
        readonly IMongoCollection<Settings> _collection;
        readonly ExecutionContext _executionContext;

        public OrganizationSettings(IMongoCollection<Settings> collection, ExecutionContext executionContext)
        {
            _collection = collection;
            _executionContext = executionContext;
        }

        [HttpGet]
        public async Task<Settings> AllSettings() => await _collection.Find(_ => _.Id == _executionContext.TenantId.ToString()).FirstOrDefaultAsync() ?? Settings.NoSettings;
    }
}
