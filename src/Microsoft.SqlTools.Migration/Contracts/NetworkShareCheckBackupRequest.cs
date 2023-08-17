//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//


using System.Collections.Generic;
using Microsoft.SqlTools.Hosting.Protocol.Contracts;

namespace Microsoft.SqlTools.Migration.Contracts
{

    public class NetworkShareCheckBackupParams
    {
        public string? NetworkSharePath { get; set; }

        public string? NetworkShareUserName { get; set; }

        public string? NetworkShareDomain { get; set; }

        public string? NetworkSharePassword { get; set; }
    }

    public class NetworkShareCheckBackupResult
    {
        public List<string> FileNames { get; set; } = new List<string>();
    }

    public class NetworkShareCheckBackupRequest
    {
        public static readonly RequestType<NetworkShareCheckBackupParams, NetworkShareCheckBackupResult> Type =
            RequestType<NetworkShareCheckBackupParams, NetworkShareCheckBackupResult>.Create("migration/networksharecheckbackup");
    }
}
