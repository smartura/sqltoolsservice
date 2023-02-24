﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using Microsoft.SqlTools.Hosting.Protocol.Contracts;

using Microsoft.SqlTools.ServiceLayer.Utility;
namespace Microsoft.SqlTools.ServiceLayer.SqlProjects.Contracts
{
    /// <summary>
    /// Move a pre-deployment script in a project
    /// </summary>
    public class MovePreDeploymentScriptRequest
    {
        public static readonly RequestType<MoveItemParams, ResultStatus> Type = RequestType<MoveItemParams, ResultStatus>.Create("sqlProjects/movePreDeploymentScript");
    }
}
