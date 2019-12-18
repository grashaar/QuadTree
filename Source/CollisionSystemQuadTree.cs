using UnityEngine;

namespace Peril.Physics
{
    /// <summary>
    /// Queries a QuadTree to test for collisions with only nearby bodies
    /// </summary>
    public class CollisionSystemQuadTree : CollisionSystem
    {
        private readonly QuadTree quadTree;

        public CollisionSystemQuadTree(QuadTree tree)
        {
            this.quadTree = tree;
        }

        public override void DetectBodyVsBody()
        {
            for(var i = 0; i < this.bodyList.Count; i++)
            {
                if (this.bodyList[i].Sleeping)
                    continue;

                // todo: something better maybe?
                var maxDist = this.bodyList[i].CollisionShape.Extents.x;
                maxDist = Mathf.Max(maxDist, this.bodyList[i].CollisionShape.Extents.y);
                maxDist = Mathf.Max(maxDist, this.bodyList[i].CollisionShape.Extents.z);

                var ents = this.quadTree.GetBodies(this.bodyList[i].CollisionShape.Center, maxDist);
                for(var j = 0; j < ents.Count; j++)
                {
                    if (!(ents[j] is ICollisionBody body2))
                        continue;

                    if (body2.Sleeping || ReferenceEquals(this.bodyList[i], body2))
                        continue;

                    Test(this.bodyList[i], body2);
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

