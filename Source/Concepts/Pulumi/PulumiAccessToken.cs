// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Concepts.Pulumi
{
    public record PulumiAccessToken(string Value) : ConceptAs<string>(Value)
    {
        public static implicit operator PulumiAccessToken(string value) => new(value);
    }
}