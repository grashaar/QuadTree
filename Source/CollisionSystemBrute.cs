/**
 *
 * Author: Jake.E
 * Purpose: Implements a brute-force algorithm for testing collisions
 *
 **/

using UnityEngine;

namespace Peril.Physics
{
    /// <summary>
    /// Brute force collision detection algorithm.
    /// </summary>
    public class CollisionSystemBrute : CollisionSystem
    {
        public override void DetectBodyVsBody()
        {
            var count = this.bodyList.Count;
            for (var i = 0; i < count; i++)
            {
                if (this.bodyList[i].Sleeping)
                    continue;

                for (var j = i + 1; j < count; j++)
                {
                    if (this.bodyList[j].Sleeping)
                        continue;

                    Test(this.bodyList[i], this.bodyList[j]);
                }
            }
        }

        public override bool LineOfSight(Vector3 start, Vector3 end)
        {
            for (var i = 0; i < this.bodyList.Count; i++)
            {
                if (CollisionTest.SegmentIntersects(this.bodyList[i].CollisionShape, start, end))
                    return false;
            }
            return true;
        }
    }
}

