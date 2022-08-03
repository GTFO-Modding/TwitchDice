using AIGraph;
using System.Collections.Generic;

namespace TwitchDice.Utilities
{
    public static class NodeUtil
    {
        public static List<AIG_CourseNode> GetReachableNodes(AIG_CourseNode from, int maxNodeDistance)
        {
            AIG_SearchID.IncrementSearchID();
            List<AIG_CourseNode> nodes = new List<AIG_CourseNode>();
            ushort searchID = AIG_SearchID.SearchID;
            Queue<AIG_CourseNode> queue = new Queue<AIG_CourseNode>();
            from.m_searchID = searchID;
            from.m_searchStep = 0;
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                AIG_CourseNode aig_CourseNode = queue.Dequeue();
                if (aig_CourseNode.m_searchStep + 1 <= maxNodeDistance)
                {
                    nodes.Add(aig_CourseNode);
                    for (int i = 0; i < aig_CourseNode.m_portals.Count; i++)
                    {
                        AIG_CoursePortal aig_CoursePortal = aig_CourseNode.m_portals[i];
                        if (aig_CoursePortal.IsTraversable && aig_CoursePortal.m_searchID != searchID)
                        {
                            aig_CoursePortal.m_searchID = searchID;
                            AIG_CourseNode oppositeNode = aig_CoursePortal.GetOppositeNode(aig_CourseNode);
                            if (oppositeNode.m_searchID != searchID)
                            {
                                oppositeNode.m_searchID = searchID;
                                oppositeNode.m_searchStep = aig_CourseNode.m_searchStep + 1;
                                queue.Enqueue(oppositeNode);
                            }
                        }
                    }
                }
            }
            return nodes;
        }
    }
}
