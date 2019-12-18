using UnityEngine;

namespace Peril.Physics
{
    public class BoxShape : ICollisionShape
    {
        private Bounds bounds;

        public Bounds Bounds
        {
            get => this.bounds;
            set => this.bounds = value;
        }

        public bool TwoD { get; set; }

        public Vector3 Center
        {
            get => this.Bounds.center;
            set => this.bounds.center = value;
        }

        public Vector3 Extents
        {
            get => this.Bounds.extents;
            set => this.bounds.extents = value;
        }

        public Vector3 Min
            => this.Center - this.Extents;

        public Vector3 Max
            => this.Center + this.Extents;

        public BoxShape(Bounds bounds, bool twoD = true)
        {
            this.Bounds = bounds;
            this.TwoD = twoD;
        }

        public CollisionResult TestCollision(ICollisionShape other)
        {
            var result = new CollisionResult();

            if (other is BoxShape box)
            {
                result.Collides = BoxVsBox(this, box, ref result, this.TwoD);
            }
            else
            {
                Debug.LogErrorFormat("Collision test not implemented: {0}-{1}", GetType(), other.GetType());
            }

            return result;
        }

        public static bool BoxVsBox(BoxShape a, BoxShape b, ref CollisionResult result, bool twoD)
        {
            return CollisionTest.TestAABB(a.Min, a.Max, b.Min, b.Max, ref result, twoD);
        }
    }
}
