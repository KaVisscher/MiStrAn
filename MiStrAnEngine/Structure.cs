﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SF = MiStrAnEngine.StaticFunctions;

namespace MiStrAnEngine
{
    public class Structure 
    {
        List<Node> nodes;
        List<ShellElement> elements;
        List<DistributedLoad> distLoads;
        List<Support> supports;
        List<PointLoad> loads;
        public SparseMatrix K;
        public Vector f;
        public Matrix bc;
        public Matrix DB;
        public Matrix T;

        public Structure(List<Node> _nodes, List<ShellElement> _elements, List<Support> _bcs, List<PointLoad> _loads, List<DistributedLoad> _distLoads)
        {
            nodes = _nodes;
            elements = _elements;
            supports = _bcs;
            loads = _loads;
            distLoads = _distLoads;
        }

        public Structure(List<Node> _nodes, List<ShellElement> _elements) : this()
        {
            nodes = _nodes;
            elements = _elements;
        }

        public Structure()
        {
            InitializeLists();
        }

        private void InitializeLists()
        {
            nodes = new List<Node>();
            elements = new List<ShellElement>();
            distLoads = new List<DistributedLoad>();
            supports = new List<Support>();
            loads = new List<PointLoad>();
        }

        public void AssembleKfbc()
        {

            int nDofs = nodes.Count * 6;
            K = new SparseMatrix(nDofs, nDofs);
            f = new Vector(nDofs);
            DB = Matrix.ZeroMatrix(elements.Count * 6,15);
            T = Matrix.ZeroMatrix(elements.Count * 18, 18);


            //Maybe make B calcs a seperate function


            for (int i=0; i<elements.Count; i++)
            {
                int[] dofs = elements[i].GetElementDofs();
                Matrix Ke, q,DBe, Te;
                Vector fe;
                elements[i].Getq(distLoads, out q);
                elements[i].q = q;
                elements[i].GenerateKefe(out Ke, out fe, out DBe, out Te);
                DB[SF.intSrs(i * 6, 5 * (i + 1) + i), SF.intSrs(0, 14)] = DBe;
                T[SF.intSrs(i * 18, 17 * (i + 1) + i), SF.intSrs(0, 17)] = Te;
                K.AddStiffnessContribution(Ke, dofs);
                f[dofs] = f[dofs] + fe;

            }




            // #TODO make it possible to have rotational loads
            foreach (PointLoad load in loads)
            {
                int[] loadDofs = new int[] {load.node.dofX, load.node.dofY, load.node.dofZ };
                f[loadDofs] = f[loadDofs] + load.LoadVec.ToVector();
            }


            List<int> fixedDofs = new List<int>();

            foreach (Support s in supports)
            {
                if (s.X) fixedDofs.Add(s.node.dofX);
                if (s.Y) fixedDofs.Add(s.node.dofY);
                if (s.Z) fixedDofs.Add(s.node.dofZ);
                if (s.RX) fixedDofs.Add(s.node.dofXX);
                if (s.RY) fixedDofs.Add(s.node.dofYY);
                if (s.RZ) fixedDofs.Add(s.node.dofZZ);
            }

            bc = Matrix.ZeroMatrix(fixedDofs.Count, 2);

            for (int i = 0; i < fixedDofs.Count; i++)
                bc[i, 0] = fixedDofs[i];
        }

        public bool Analyze(out Vector a, out Vector r, bool useExactMethod = false)
        {
            AssembleKfbc();


            return StaticFunctions.solveq(K, f, bc, out a, out r, useExactMethod);


        }

        //Calculate principal tresses for all the elements
        public void CalcStresses(List<double> a, out List<Vector3D> principalStresses)
        {
            //Folllowing plants in MATlab, each element 15 dofs

            //the 15 active dofs used in each element (with 18 dofs in total)
            int[] activeDofs = new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16 };
            principalStresses = new List<Vector3D>();

            for (int i =0; i<elements.Count; i++)
            {
                //Get all the 18 element dofs from the global
                int[] dofsFull = elements[i].GetElementDofs();

                //Take the deformations from the element dofs
                Matrix aTrans = new Matrix(18, 1);
                for (int j = 0; j < dofsFull.Count(); j++)
                    aTrans[j] = a[dofsFull[j]];

                //Transform the local coordinates to global
                aTrans = elements[i].Te.Invert()* aTrans; //tror inte invert behövs

                //Take the 15 active dofs that should be used from the transformed defrormations 
                Matrix ed = new Matrix(1, 15);
                for (int j =0; j< activeDofs.Count();j++)
                    ed[0, j] = aTrans[activeDofs[j]];

                    
                //Stresses (D*B*a)
                Matrix ss =elements[i].DBe * ed[0, SF.intSrs(0, 14)].Transpose();

                //Von mises
                //   double vM = Math.Sqrt(Math.Pow(ss[i,0],2)+ Math.Pow(ss[i,1], 2)-ss[i,0]*ss[i,1]+3*Math.Pow(ss[i,2],2));
                //  vMstresses.Add(vM);

                //Principle stresses
                double p1 = (ss[0] + ss[1]) / 2.0 + Math.Sqrt(Math.Pow((ss[0]-ss[1])/2,2)+Math.Pow(ss[2],2));
                double p2 = (ss[0] + ss[1]) / 2.0 - Math.Sqrt(Math.Pow((ss[0] - ss[1]) / 2, 2) + Math.Pow(ss[2], 2));
                principalStresses.Add(new Vector3D(p1, p2, 0));
            }

        }

        public void AddSupports(List<Support> supports)
        {
            foreach (Support s in supports)
                this.AddSupport(s);
        }

        public void AddSupport(Support support)
        {
            Node node = GetOrAddNode(support.node.Pos);
            support.node = node;
            supports.Add(support);
        }

        public Node GetOrAddNode(Vector3D pos)
        {
            double tol = 0.001;

            for (int i = 0; i < nodes.Count; i++)
            {
                double dist = (nodes[i].Pos - pos).Length;
                if (dist < tol)
                    return nodes[i];
            }

            Node newNode = new Node(pos.X, pos.Y, pos.Z, nodes.Count);
            nodes.Add(newNode);

            return newNode;
        }

        public override string ToString()
        {
            return "MiStrAn Strucuture: Nodes: " + nodes.Count + ", Elements: " + elements.Count + ", Supports: " + supports.Count;
        }
    }
}
