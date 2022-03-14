﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlTools.ServiceLayer.ShowPlan.Contracts;

namespace Microsoft.SqlTools.ServiceLayer.ShowPlan.ShowPlanGraph.Comparison
{
    public class SkeletonNode
    {
        public Node BaseNode { get; set; }
        public List<SkeletonNode> MatchingNodes { get; set; }
        public bool HasMatch { get { return MatchingNodes.Count > 0; } }
        public SkeletonNode ParentNode { get; set; }
        public IList<SkeletonNode> Children { get; set; }

        public int GroupIndex
        {
            get
            {
                return this.BaseNode.GroupIndex;
            }
            set
            {
                this.BaseNode.GroupIndex = value;
            }
        }

        public SkeletonNode(Node baseNode)
        {
            baseNode[NodeBuilderConstants.SkeletonNode] = this;
            this.BaseNode = baseNode;
            this.Children = new List<SkeletonNode>();
            this.MatchingNodes = new List<SkeletonNode>();
        }

        /// <summary>
        /// Adds node to children collection and sets this node as parent of the child
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(SkeletonNode child)
        {
            child.ParentNode = this;
            this.Children.Add(child);
        }

        public void ChangeSkeletonGroupIndex(int groupIndex)
        {
            this.GroupIndex = groupIndex;
            foreach (SkeletonNode child in this.Children)
            {
                child.ChangeSkeletonGroupIndex(groupIndex);
            }
        }

        public void AddMatchingSkeletonNode(SkeletonNode match, bool ignoreDatabaseName, bool matchAllChildren = true)
        {
            this.BaseNode[NodeBuilderConstants.SkeletonHasMatch] = true;
            if (matchAllChildren == true)
            {
                SkeletonManager manager = new SkeletonManager();
                foreach (SkeletonNode baseChild in this.Children)
                {
                    foreach (SkeletonNode matchChild in match.Children)
                    {
                        // make sure this is the right child to match
                        if (baseChild.BaseNode.IsLogicallyEquivalentTo(matchChild.BaseNode, ignoreDatabaseName))
                        {
                            baseChild.AddMatchingSkeletonNode(matchChild, ignoreDatabaseName, matchAllChildren);
                        }
                    }
                }
            }
            this.MatchingNodes.Add(match);
        }

        public Graph GetGraph()
        {
            return this.BaseNode.Graph;
        }

        public SkeletonNodeDTO ConvertToDTO()
        {
            var skeletonNodeDTO = new SkeletonNodeDTO();
            LevelOrderCopy(this, skeletonNodeDTO);

            return skeletonNodeDTO;
        }

        private static void LevelOrderCopy(SkeletonNode skeletonNode, SkeletonNodeDTO skeletonNodeDTO)
        {
            if (skeletonNode == null)
                return;

            var queue = new Queue<SkeletonNode>();
            queue.Enqueue(skeletonNode);

            var dtoQueue = new Queue<SkeletonNodeDTO>();
            dtoQueue.Enqueue(skeletonNodeDTO);

            while (queue.Count != 0)
            {
                var curNode = queue.Dequeue();
                var curNodeDTO = dtoQueue.Dequeue();

                curNodeDTO.BaseNode = curNode.BaseNode.ConvertToDTO();
                curNodeDTO.GroupIndex = curNode.GroupIndex;
                curNodeDTO.HasMatch = curNode.HasMatch;
                curNodeDTO.MatchingNodes = curNode.MatchingNodes.Select(matchingNode =>
                                            {
                                                var skeletonNodeDTO = new SkeletonNodeDTO();
                                                skeletonNodeDTO.BaseNode = matchingNode.BaseNode.ConvertToDTO();

                                                return skeletonNodeDTO;
                                            }).ToList();

                foreach (var child in curNode.Children)
                {
                    queue.Enqueue(child);

                    var childDTO = new SkeletonNodeDTO();
                    childDTO.ParentNode = curNodeDTO;
                    curNodeDTO.Children.Add(childDTO);
                    dtoQueue.Enqueue(childDTO);
                }
            }
        }
    }
}
