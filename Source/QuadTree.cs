using System;
using System.Collections.Generic;
using UnityEngine;

namespace Peril.Physics
{
    public class QuadTree
    {
        private static class QuadTreePool
        {
            private const int _maxPoolCount = 1024;
            private const int _defaultMaxBodiesPerNode = 6;
            private const int _defaultMaxLevel = 6;

            private static Queue<QuadTree> _pool;

            public static QuadTree GetQuadTree(Rect bounds, QuadTree parent)
            {
                if (_pool == null) Init();

                QuadTree tree;

                if (_pool.Count > 0)
                {
                    tree = _pool.Dequeue();
                    tree.bounds = bounds;
                    tree.parent = parent;
                    tree.maxLevel = parent.maxLevel;
                    tree.maxBodiesPerNode = parent.maxBodiesPerNode;
                    tree.curLevel = parent.curLevel + 1;
                }
                else tree = new QuadTree(bounds, parent);
                return tree;
            }

            public static void PoolQuadTree(QuadTree tree)
            {
                if (tree == null) return;
                tree.Clear();
                if (_pool.Count > _maxPoolCount) return;
                _pool.Enqueue(tree);
            }

            private static void Init()
            {
                _pool = new Queue<QuadTree>();
                for (var i = 0; i < _maxPoolCount; i++)
                {
                    _pool.Enqueue(new QuadTree(Rect.zero, _defaultMaxBodiesPerNode, _defaultMaxLevel));
                }
            }
        }

        private readonly List<IQuadTreeBody> bodies;

        private QuadTree parent;
        private Rect bounds;
        private int maxBodiesPerNode;
        private int maxLevel;
        private int curLevel;
        private QuadTree childA;
        private QuadTree childB;
        private QuadTree childC;
        private QuadTree childD;
        private List<IQuadTreeBody> entCache;

        public QuadTree(Rect bounds, int maxBodiesPerNode = 6, int maxLevel = 6)
        {
            this.bounds = bounds;
            this.maxBodiesPerNode = maxBodiesPerNode;
            this.maxLevel = maxLevel;
            this.bodies = new List<IQuadTreeBody>(maxBodiesPerNode);
        }

        private QuadTree(Rect bounds, QuadTree parent)
            : this(bounds, parent.maxBodiesPerNode, parent.maxLevel)
        {
            this.parent = parent;
            this.curLevel = parent.curLevel + 1;
        }

        public void AddBody(IQuadTreeBody body)
        {
            if (this.childA != null)
            {
                var child = GetQuadrant(body.Position);
                child.AddBody(body);
            }
            else
            {
                this.bodies.Add(body);
                if (this.bodies.Count > this.maxBodiesPerNode && this.curLevel < this.maxLevel)
                {
                    Split();
                }
            }
        }

        public List<IQuadTreeBody> GetBodies(Vector3 point, float radius)
        {
            var p = new Vector2(point.x, point.z);
            return GetBodies(p, radius);
        }

        public List<IQuadTreeBody> GetBodies(Vector2 point, float radius)
        {
            if (this.entCache == null) this.entCache = new List<IQuadTreeBody>(64);
            else this.entCache.Clear();
            GetBodies(point, radius, this.entCache);
            return this.entCache;
        }

        public List<IQuadTreeBody> GetBodies(Rect rect)
        {
            if (this.entCache == null) this.entCache = new List<IQuadTreeBody>(64);
            else this.entCache.Clear();
            GetBodies(rect, this.entCache);
            return this.entCache;
        }

        private void GetBodies(Vector2 point, float radius, List<IQuadTreeBody> bods)
        {
            //no children
            if (this.childA == null)
            {
                for (var i = 0; i < this.bodies.Count; i++)
                    bods.Add(this.bodies[i]);
            }
            else
            {
                if (this.childA.ContainsCircle(point, radius))
                    this.childA.GetBodies(point, radius, bods);
                if (this.childB.ContainsCircle(point, radius))
                    this.childB.GetBodies(point, radius, bods);
                if (this.childC.ContainsCircle(point, radius))
                    this.childC.GetBodies(point, radius, bods);
                if (this.childD.ContainsCircle(point, radius))
                    this.childD.GetBodies(point, radius, bods);
            }
        }

        private void GetBodies(Rect rect, List<IQuadTreeBody> bods)
        {
            //no children
            if (this.childA == null)
            {
                for (var i = 0; i < this.bodies.Count; i++)
                    bods.Add(this.bodies[i]);
            }
            else
            {
                if (this.childA.ContainsRect(rect))
                    this.childA.GetBodies(rect, bods);
                if (this.childB.ContainsRect(rect))
                    this.childB.GetBodies(rect, bods);
                if (this.childC.ContainsRect(rect))
                    this.childC.GetBodies(rect, bods);
                if (this.childD.ContainsRect(rect))
                    this.childD.GetBodies(rect, bods);
            }
        }

        public bool ContainsCircle(Vector2 circleCenter, float radius)
        {
            var center = this.bounds.center;
            var dx = Math.Abs(circleCenter.x - center.x);
            var dy = Math.Abs(circleCenter.y - center.y);
            if (dx > (this.bounds.width / 2 + radius)) { return false; }
            if (dy > (this.bounds.height / 2 + radius)) { return false; }
            if (dx <= (this.bounds.width / 2)) { return true; }
            if (dy <= (this.bounds.height / 2)) { return true; }
            var cornerDist = Math.Pow((dx - this.bounds.width / 2), 2) + Math.Pow((dy - this.bounds.height / 2), 2);
            return cornerDist <= (radius * radius);
        }

        public bool ContainsRect(Rect rect)
        {
            return this.bounds.Overlaps(rect);
        }

        private QuadTree GetLowestChild(Vector2 point)
        {
            var ret = this;
            while (ret != null)
            {
                var newChild = ret.GetQuadrant(point);
                if (newChild != null) ret = newChild;
                else break;
            }
            return ret;
        }

        public void Clear()
        {
            QuadTreePool.PoolQuadTree(this.childA);
            QuadTreePool.PoolQuadTree(this.childB);
            QuadTreePool.PoolQuadTree(this.childC);
            QuadTreePool.PoolQuadTree(this.childD);
            this.childA = null;
            this.childB = null;
            this.childC = null;
            this.childD = null;
            this.bodies.Clear();
        }

        public void DrawGizmos()
        {
            //draw children
            if (this.childA != null) this.childA.DrawGizmos();
            if (this.childB != null) this.childB.DrawGizmos();
            if (this.childC != null) this.childC.DrawGizmos();
            if (this.childD != null) this.childD.DrawGizmos();

            //draw rect
            Gizmos.color = Color.cyan;
            var p1 = new Vector3(this.bounds.position.x, 0.1f, this.bounds.position.y);
            var p2 = new Vector3(p1.x + this.bounds.width, 0.1f, p1.z);
            var p3 = new Vector3(p1.x + this.bounds.width, 0.1f, p1.z + this.bounds.height);
            var p4 = new Vector3(p1.x, 0.1f, p1.z + this.bounds.height);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }

        private void Split()
        {
            var hx = this.bounds.width / 2;
            var hz = this.bounds.height / 2;
            var sz = new Vector2(hx, hz);

            //split a
            var aLoc = this.bounds.position;
            var aRect = new Rect(aLoc, sz);
            //split b
            var bLoc = new Vector2(this.bounds.position.x + hx, this.bounds.position.y);
            var bRect = new Rect(bLoc, sz);
            //split c
            var cLoc = new Vector2(this.bounds.position.x + hx, this.bounds.position.y + hz);
            var cRect = new Rect(cLoc, sz);
            //split d
            var dLoc = new Vector2(this.bounds.position.x, this.bounds.position.y + hz);
            var dRect = new Rect(dLoc, sz);

            //assign QuadTrees
            this.childA = QuadTreePool.GetQuadTree(aRect, this);
            this.childB = QuadTreePool.GetQuadTree(bRect, this);
            this.childC = QuadTreePool.GetQuadTree(cRect, this);
            this.childD = QuadTreePool.GetQuadTree(dRect, this);

            for (var i = this.bodies.Count - 1; i >= 0; i--)
            {
                var child = GetQuadrant(this.bodies[i].Position);
                child.AddBody(this.bodies[i]);
                this.bodies.RemoveAt(i);
            }
        }

        private QuadTree GetQuadrant(Vector2 point)
        {
            if (this.childA == null) return null;
            if (point.x > this.bounds.x + this.bounds.width / 2)
            {
                if (point.y > this.bounds.y + this.bounds.height / 2) return this.childC;
                else return this.childB;
            }
            else
            {
                if (point.y > this.bounds.y + this.bounds.height / 2) return this.childD;
                return this.childA;
            }
        }
    }
}