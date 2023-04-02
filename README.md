# AStarPathFind_3D
 
This code was created on the assumption that

- The movement of the road is parallel to the x, y, and z axes except for diagonal movement

===========================================================================

The sample code for using the library is as follows
```
using System;
using System.Collections.Generic;

using UnityEngine;

namespace Assets.Scripts
{
    internal class AStar : MonoBehaviour
    {
        public Vector3Int size, startPos, endPos;
        public GameObject cube, wall;
        public List<Vector3Int> collisionList;

        private List<AStarLib.AStarNode> closeNodeList;

        private void Awake()
        {
            foreach (Vector3Int pos in  collisionList)
            {
                Instantiate(wall, pos, Quaternion.identity);
            }

            AStarLib.TryReach(size, startPos, endPos, collisionList, out closeNodeList);

            foreach (var node in closeNodeList)
            {
                Instantiate(cube, node.pos, Quaternion.identity);
            }

            // or use this
            //AStarLib.AStarNode curNode = closeNodeList.Last();
            //while (true)
            //{
            //    Instantiate(cube, curNode.pos, Quaternion.identity);

            //    if (curNode.parentNode == null)
            //    {
            //        break;
            //    }
            //    curNode = curNode.parentNode.Item1;
            //}
        }
    }
}
```

![1](https://user-images.githubusercontent.com/63137431/229336990-ff27382e-6981-46b7-ab4a-b11e6ef7afbc.png)
![2](https://user-images.githubusercontent.com/63137431/229336995-0ea3d83d-d755-417c-8774-8a9f18d92906.png)
