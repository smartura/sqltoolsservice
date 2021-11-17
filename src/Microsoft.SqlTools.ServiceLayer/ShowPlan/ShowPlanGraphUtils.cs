//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.SqlTools.ServiceLayer.ShowPlan.ShowPlanGraph;

namespace Microsoft.SqlTools.ServiceLayer.ShowPlan
{
    public class ShowPlanGraphUtils
    {
        public static List<ExecutionPlanGraph> CreateShowPlanGraph(string xml)
        {
            ShowPlanGraph.ShowPlanGraph[] graphs = ShowPlanGraph.ShowPlanGraph.ParseShowPlanXML(xml, ShowPlanGraph.ShowPlanType.Unknown);
            return graphs.Select(g => new ExecutionPlanGraph
            {
                Root = ConvertShowPlanTreeToExecutionPlanTree(g.Root),
                Query = g.Statement
            }).ToList();
        }

        private static ExecutionPlanNode ConvertShowPlanTreeToExecutionPlanTree(Node currentNode)
        {
            return new ExecutionPlanNode
            {
                Type = currentNode.Operation.Image,
                Cost = currentNode.Cost,
                SubTreeCost = currentNode.SubtreeCost,
                Description = currentNode.Description,
                Subtext = currentNode.GetDisplayLinesOfText(),
                RelativeCost = currentNode.RelativeCost,
                Properties = GetProperties(currentNode.Properties),
                Children = currentNode.Children.Select(x => ConvertShowPlanTreeToExecutionPlanTree(x)).ToList(),
                Edges = currentNode.Edges.Select(x => ConvertShowPlanEdgeToExecutionPlanEdge(x)).ToList(),
                Name = currentNode.DisplayName,
                ElapsedTimeInMs = currentNode.ElapsedTimeInMs
            };
        }

        private static ExecutionPlanEdges ConvertShowPlanEdgeToExecutionPlanEdge(Edge edge)
        {
            return new ExecutionPlanEdges
            {
                RowCount = edge.RowCount,
                RowSize = edge.RowSize,
                Properties = GetProperties(edge.Properties)
            };
        }

        private static List<ExecutionPlanGraphElementProperties> GetProperties(PropertyDescriptorCollection props)
        {
            List<ExecutionPlanGraphElementProperties> propsList = new List<ExecutionPlanGraphElementProperties>();
            foreach (PropertyValue prop in props)
            {
                propsList.Add(new ExecutionPlanGraphElementProperties()
                {
                    Name = prop.DisplayName,
                    FormattedValue = prop.DisplayValue,
                    ShowInTooltip = prop.IsBrowsable,
                    DisplayOrder = prop.DisplayOrder,
                    IsLongString = prop.IsLongString
                });
            }
            return propsList;
        }
    }
}
