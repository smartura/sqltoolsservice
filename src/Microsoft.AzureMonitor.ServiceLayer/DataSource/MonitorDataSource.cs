using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.OperationalInsights.Models;
using Microsoft.AzureMonitor.ServiceLayer.DataSource.Client;
using Microsoft.AzureMonitor.ServiceLayer.DataSource.Client.Contracts.Responses;
using Microsoft.AzureMonitor.ServiceLayer.DataSource.Client.Contracts.Responses.Models;
using Microsoft.SqlTools.Hosting.DataContracts.Connection.Models;
using Microsoft.SqlTools.Hosting.DataContracts.ObjectExplorer.Models;

namespace Microsoft.AzureMonitor.ServiceLayer.DataSource
{
    public class MonitorDataSource
    {
        private readonly MonitorClient _monitorClient;
        private readonly WorkspaceResponse _metadata;        
        public string ServerName => _monitorClient.WorkspaceId;
        public string DatabaseName { get; private set; }
        public string UserName { get; }
        private readonly Dictionary<string, List<NodeInfo>> _nodes;

        public MonitorDataSource(MonitorClient monitorClient, string userName)
        {
            _monitorClient = monitorClient;
            UserName = userName;
            _nodes = new Dictionary<string, List<NodeInfo>>();
            _metadata = _monitorClient.LoadMetadata();
            SetupTableGroups(monitorClient.WorkspaceId);
        }
        
        private void SetupTableGroups(string workspaceId)
        {
            var workspace = _metadata.Workspaces.First(x => x.Id == workspaceId);
            DatabaseName = workspace.Name;
            var metadataTableGroups = _metadata.TableGroups.ToDictionary(x => x.Id);
            
            foreach (string workspaceTableGroup in workspace.TableGroups)
            {
                var tableGroup = metadataTableGroups[workspaceTableGroup];

                var tableGroupNodeInfo = new NodeInfo
                {
                    NodePath = $"/{tableGroup.Name}",
                    NodeType = NodeTypes.Folder.ToString(),
                    IsLeaf = false,
                    Label = tableGroup.Name,
                    Metadata = new ObjectMetadata
                    {
                        MetadataTypeName = NodeTypes.Folder.ToString(),
                        Name = tableGroup.Name,
                    }
                };

                _nodes.AddToValueList("/", tableGroupNodeInfo);
                
                SetupTables(tableGroupNodeInfo);
            }
        }
        
        private void SetupTables(NodeInfo tableGroupNodeInfo)
        {
            var tables = GetNonEmptyTableNames();
            var metadataTables = _metadata.Tables.ToDictionary(x => x.Name);
            
            foreach (string tableName in tables)
            {
                var table = metadataTables[tableName];

                var tableNodeInfo = new NodeInfo
                {
                    NodePath = $"{tableGroupNodeInfo.NodePath}/{table.Name}",
                    NodeType = NodeTypes.Table.ToString(),
                    IsLeaf = false,
                    Label = table.Name,
                    Metadata = new ObjectMetadata
                    {
                        MetadataTypeName = NodeTypes.Table.ToString(),
                        Name = table.Name,
                    }
                };

                _nodes.AddToValueList(tableGroupNodeInfo.NodePath, tableNodeInfo);

                SetupColumns(table, tableNodeInfo);
            }
        }
        
        private void SetupColumns(TablesModel table, NodeInfo tableNodeInfo)
        {
            foreach (var column in table.Columns)
            {
                var columnNodeInfo = new NodeInfo
                {
                    NodePath = $"{tableNodeInfo.NodePath}/{column.Name}",
                    NodeType = NodeTypes.Column.ToString(),
                    IsLeaf = true,
                    Label = column.Name,
                    Metadata = new ObjectMetadata
                    {
                        MetadataTypeName = NodeTypes.Column.ToString(),
                        Name = column.Name
                    }
                };

                _nodes.AddToValueList(tableNodeInfo.NodePath, columnNodeInfo);
            }
        }

        public List<ObjectMetadata> GetDatabases(bool includeSizeDetails)
        {
            return _metadata.Workspaces
                .Select(x => new ObjectMetadata
            {
                MetadataTypeName = NodeTypes.Database.ToString(),
                Name = x.Name,
                PrettyName = x.Name,
                SizeInMb = 0

            }).ToList();
        }

        private IEnumerable<string> GetNonEmptyTableNames()
        {
            string query = "union * | summarize count() by Type";
            var results = _monitorClient.Query(query);
            return results.Tables[0].Rows.Select(x => x[0]).OrderBy(x => x);
        }

        public void ChangeDatabase(string newDatabase)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NodeInfo> Expand(string nodePath)
        {
            return _nodes[nodePath].OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase);
        }

        public async Task<QueryResults> QueryAsync(string query, CancellationToken cancellationToken)
        {
            return await _monitorClient.QueryAsync(query, cancellationToken);
        }
    }
}