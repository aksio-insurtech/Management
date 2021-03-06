// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Concepts.Applications;
using Events.Applications;

namespace Domain.Applications;

[Route("/api/applications/{applicationId}/microservices/{microserviceId}/deployables")]
public class Deployables : Controller
{
    readonly IEventLog _eventLog;

    public Deployables(IEventLog eventLog) => _eventLog = eventLog;

    [HttpPost]
    public Task Create([FromRoute] MicroserviceId microserviceId, [FromBody] CreateDeployable command) => _eventLog.Append(command.DeployableId.ToString(), new DeployableCreated(microserviceId, command.Name));

    [HttpPost("{deployableId}/image")]
    public Task SetImage([FromRoute] MicroserviceId microserviceId, [FromRoute] DeployableId deployableId, [FromBody] DeployableImageName deployableImageName) => _eventLog.Append(deployableId.ToString(), new DeployableImageChanged(deployableImageName));
}
