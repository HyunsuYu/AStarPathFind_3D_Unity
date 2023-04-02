using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets.Scripts
{
    internal static class AStarLib
    {
        public struct AStarNode : IEquatable<AStarNode>
        {
            public float costFromStart, costToEnd, totalCost;
            public Vector3Int pos;
            public Tuple<AStarNode> parentNode;

            public bool Equals(AStarNode other)
            {
                return pos == other.pos;
            }
            public static bool operator ==(AStarNode left, AStarNode right)
            {
                return left.Equals(right);
            }
            public static bool operator !=(AStarNode left, AStarNode right)
            {
                return !left.Equals(right);
            }
        }


        public static bool TryReach(in Vector3Int realRoomUnitMapSize, in Vector3Int startPos, in Vector3Int endPos, in List<Vector3Int> collisionList, out List<AStarNode> closeNodeList)
        {
            foreach (Vector3Int collisionNode in collisionList)
            {
                if (collisionNode == startPos || collisionNode == endPos)
                {
                    closeNodeList = null;
                    return false;
                }
            }

            bool result = false;
            List<AStarNode> openNodeList = new List<AStarNode>();
            closeNodeList = new List<AStarNode>();
            AStarNode startNode = new AStarNode()
            {
                costFromStart = 0,
                costToEnd = (endPos.x - startPos.x + endPos.y - startPos.y + endPos.z - startPos.z),
                pos = startPos,
                parentNode = null
            };
            startNode.totalCost = startNode.costFromStart + startNode.costToEnd;

            openNodeList.Add(startNode);
            closeNodeList.Add(startNode);

            List<AStarNode> nearNodes = null;
            FindNearByNodes(realRoomUnitMapSize, startPos, endPos, closeNodeList.Last(), openNodeList, closeNodeList, collisionList, out nearNodes);

            while (true)
            {
                if (nearNodes.Count == 0)
                {
                    for (int index = closeNodeList.Count - 1; index >= 0; index--)
                    {
                        FindNearByNodes(realRoomUnitMapSize, startPos, endPos, closeNodeList[index], openNodeList, closeNodeList, collisionList, out nearNodes);
                        if (nearNodes.Count > 0)
                        {
                            break;
                        }

                        if (index == 0 && nearNodes.Count == 0)
                        {
                            closeNodeList = null;
                            return false;
                        }
                    }
                }
                else
                {
                    var nextWayNode = FIndSmallestTotalCoseAStarNode(nearNodes, closeNodeList);
                    closeNodeList.Add(nextWayNode);

                    if (nextWayNode.pos == endPos)
                    {
                        break;
                    }

                    FindNearByNodes(realRoomUnitMapSize, startPos, endPos, closeNodeList.Last(), openNodeList, closeNodeList, collisionList, out nearNodes);
                }
            }

            // Find Shortest Path
            AStarNode curNode = closeNodeList.Last();
            int curIndex = closeNodeList.Count - 1;
            List<AStarNode> shortestPathList = new List<AStarNode>();
            while (true)
            {
                bool bisNoShortWay = true;

                for (int index = curIndex - 1; index >= 0; index--)
                {
                    if (curNode.costFromStart > closeNodeList[index].costFromStart && (Mathf.Abs(curNode.pos.x - closeNodeList[index].pos.x) + Mathf.Abs(curNode.pos.y - closeNodeList[index].pos.y) + Mathf.Abs(curNode.pos.z - closeNodeList[index].pos.z)) == 1)
                    {
                        curIndex = index;

                        bisNoShortWay = false;

                        break;
                    }
                }

                if (bisNoShortWay)
                {
                    curIndex--;
                    shortestPathList.Add(curNode);
                    curNode = closeNodeList[curIndex];
                }
                else
                {
                    curNode.parentNode = new Tuple<AStarNode>(closeNodeList[curIndex]);
                    shortestPathList.Add(curNode);
                    curNode = closeNodeList[curIndex];
                }

                if (curNode == startNode)
                {
                    shortestPathList.Add(curNode);
                    break;
                }
            }
            shortestPathList.Reverse();
            closeNodeList = shortestPathList;

            return result;
        }

        private static List<AStarNode> FindNearByNodes(in Vector3Int realRoomUnitMapSize, in Vector3Int startPos, in Vector3Int endPos, in AStarNode parentNode, in List<AStarNode> openNodeList, in List<AStarNode> closeNodeList, in List<Vector3Int> collisionList, out List<AStarNode> nearNodes/*, in Tuple<int, int> allowedAreaChunkUIDs*/)
        {
            List<AStarNode> alreadyExistNodes = new List<AStarNode>();
            nearNodes = new List<AStarNode>();
            Vector3Int tryPos = new Vector3Int();

            // x
            tryPos.Set(parentNode.pos.x - 1, parentNode.pos.y, parentNode.pos.z);
            if (tryPos.x >= 0 && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            tryPos.Set(parentNode.pos.x + 1, parentNode.pos.y, parentNode.pos.z);
            if (tryPos.x < realRoomUnitMapSize.x && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            // y
            tryPos.Set(parentNode.pos.x, parentNode.pos.y - 1, parentNode.pos.z);
            if (tryPos.y >= 0 && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            tryPos.Set(parentNode.pos.x, parentNode.pos.y + 1, parentNode.pos.z);
            if (tryPos.y < realRoomUnitMapSize.y && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            // z
            tryPos.Set(parentNode.pos.x, parentNode.pos.y, parentNode.pos.z - 1);
            if (tryPos.z >= 0 && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            tryPos.Set(parentNode.pos.x, parentNode.pos.y, parentNode.pos.z + 1);
            if (tryPos.z < realRoomUnitMapSize.z && !collisionList.Contains(tryPos))
            {
                AddAStarNodeInOpenNodeList(tryPos, startPos, endPos, parentNode, openNodeList, closeNodeList, alreadyExistNodes, nearNodes);
            }

            return alreadyExistNodes;
        }
        private static void AddAStarNodeInOpenNodeList(in Vector3Int tryPos, in Vector3Int StartPos, in Vector3Int endPos, in AStarNode parentNode, in List<AStarNode> openNodeList, in List<AStarNode> closeNodeList, in List<AStarNode> alreadyExistNodes, in List<AStarNode> nearNodes)
        {
            AStarNode aStarNode = new AStarNode()
            {
                costFromStart = (Mathf.Abs(tryPos.x - StartPos.x) + Mathf.Abs(tryPos.y - StartPos.y) + Mathf.Abs(tryPos.z - StartPos.z)),
                costToEnd = (Mathf.Abs(tryPos.x - endPos.x) + Mathf.Abs(tryPos.y - endPos.y) + Mathf.Abs(tryPos.z - endPos.z)),
                pos = tryPos,
                parentNode = new Tuple<AStarNode>(parentNode)
            };
            aStarNode.totalCost = aStarNode.costFromStart + aStarNode.costToEnd;

            if (openNodeList.Contains(aStarNode))
            {
                if (!closeNodeList.Contains(aStarNode))
                {
                    nearNodes.Add(aStarNode);
                }
            }
            else
            {
                openNodeList.Add(aStarNode);
                nearNodes.Add(aStarNode);
            }

            if (closeNodeList.Contains(aStarNode))
            {
                alreadyExistNodes.Add(aStarNode);
            }
        }
        private static AStarNode FIndSmallestTotalCoseAStarNode(in List<AStarNode> nearNodes, in List<AStarNode> closeNodeList)
        {
            int resultIndex = 0;

            for (int index = resultIndex + 1; index < nearNodes.Count; index++)
            {
                if (!closeNodeList.Contains(nearNodes[index]) && nearNodes[resultIndex].totalCost >= nearNodes[index].totalCost)
                {
                    if (nearNodes[resultIndex].totalCost == nearNodes[index].totalCost)
                    {
                        resultIndex = (nearNodes[resultIndex].costToEnd > nearNodes[index].costToEnd) ? index : resultIndex;
                    }
                    else
                    {
                        resultIndex = index;
                    }
                }
            }

            return nearNodes[resultIndex];
        }
    }
}
