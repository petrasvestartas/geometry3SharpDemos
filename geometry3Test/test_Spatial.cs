﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using g3;

namespace geometry3Test
{
    public static class test_Spatial
    {

        public static DMesh3 MakeSpatialTestMesh(int n)
        {
            if (n == 0)
                return TestUtil.MakeCappedCylinder(false);
            else if (n == 1)
                return TestUtil.MakeCappedCylinder(true);
            else if (n == 2)
                return TestUtil.MakeCappedCylinder(false, 256);
            else if (n == 3)
                return TestUtil.MakeCappedCylinder(true, 256);
            else if (n == 4)
                return TestUtil.MakeRemeshedCappedCylinder(1.0f);
            else if (n == 5)
                return TestUtil.MakeRemeshedCappedCylinder(0.5f);
            else if (n == 6)
                return TestUtil.MakeRemeshedCappedCylinder(0.25f);
            else if (n == 7)
                return TestUtil.MakeCappedCylinder(false, 128, true);
            throw new Exception("test_Spatial.MakeSpatialTestMesh: unknown mesh case");
        }
        public static int NumTestCases { get { return 7; } }



        public static void test_AABBTree_basic()
        {
            List<int> cases = new List<int>() { 0, 1, 2, 3, 4, 7 };

            foreach (int meshCase in cases) {

                DMesh3 mesh = MakeSpatialTestMesh(meshCase);
                DMeshAABBTree3 treeMedian = new DMeshAABBTree3(mesh);
                treeMedian.Build(DMeshAABBTree3.BuildStrategy.TopDownMedian);
                treeMedian.TestCoverage();
                treeMedian.TotalVolume();

                DMeshAABBTree3 treeMidpoint = new DMeshAABBTree3(mesh);
                treeMidpoint.Build(DMeshAABBTree3.BuildStrategy.TopDownMidpoint);
                treeMidpoint.TestCoverage();
                treeMidpoint.TotalVolume();

                DMeshAABBTree3 treeUpFast = new DMeshAABBTree3(mesh);
                treeUpFast.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.Fastest);
                treeUpFast.TestCoverage();
                treeUpFast.TotalVolume();

                DMeshAABBTree3 treeUpN = new DMeshAABBTree3(mesh);
                treeUpN.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.FastVolumeMetric);
                treeUpN.TestCoverage();
                treeUpN.TotalVolume();
            }
        }



        public static void test_AABBTree_TriDist()
        {
            int meshCase = 0;
            DMesh3 mesh = MakeSpatialTestMesh(meshCase);
            DMeshAABBTree3 tree = new DMeshAABBTree3(mesh);
            tree.Build();

            AxisAlignedBox3d bounds = mesh.CachedBounds;
            Vector3d ext = bounds.Extents;
            Vector3d c = bounds.Center;

            Random rand = new Random(316136327);

            int N = 10000;
            for ( int ii = 0; ii < N; ++ii ) {
                Vector3d p = new Vector3d(
                    c.x + (4 * ext.x * (2 * rand.NextDouble() - 1)),
                    c.y + (4 * ext.y * (2 * rand.NextDouble() - 1)),
                    c.z + (4 * ext.z * (2 * rand.NextDouble() - 1)));

                int tNearBrute = MeshQueries.FindNearestTriangle_LinearSearch(mesh, p);
                int tNearTree = tree.FindNearestTriangle(p);

                DistPoint3Triangle3 qBrute = MeshQueries.TriangleDistance(mesh, tNearBrute, p);
                DistPoint3Triangle3 qTree = MeshQueries.TriangleDistance(mesh, tNearTree, p);

                if ( Math.Abs(qBrute.DistanceSquared - qTree.DistanceSquared) > MathUtil.ZeroTolerance )
                    Util.gBreakToDebugger();
            }
        }



        public static void test_AABBTree_profile()
        {
            System.Console.WriteLine("Building test meshes");
            DMesh3[] meshes = new DMesh3[NumTestCases];
            for ( int i = 0; i < NumTestCases; ++i )
                meshes[i] = MakeSpatialTestMesh(i);
            System.Console.WriteLine("done!");


            int N = 50;

            // avoid garbage collection
            List<DMeshAABBTree3> trees = new List<DMeshAABBTree3>();
            DMeshAABBTree3 tree = null;



            for (int i = 0; i < NumTestCases; ++i) {
                Stopwatch w = new Stopwatch();
                for (int j = 0; j < N; ++j) {
                    tree = new DMeshAABBTree3(meshes[i]);
                    w.Start();
                    tree.Build(DMeshAABBTree3.BuildStrategy.TopDownMidpoint);
                    //tree.Build(DMeshAABBTree3.BuildStrategy.TopDownMedian);
                    //tree.Build(DMeshAABBTree3.BuildStrategy.BottomUpFromOneRings, DMeshAABBTree3.ClusterPolicy.FastVolumeMetric);
                    w.Stop();
                    trees.Add(tree);
                }
                double avg_time = w.ElapsedTicks / (double)N;
                System.Console.WriteLine(string.Format("Case {0}: time {1}  tris {2} vol {3}  len {4}", i, avg_time, tree.Mesh.TriangleCount, tree.TotalVolume(), tree.TotalExtentSum()));
            }

        }


    }
}
