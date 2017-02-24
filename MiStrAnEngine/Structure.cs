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
        List<BC> bcs;
        List<Load> loads;
        public SparseMatrix K;
        public Vector f;
        public Matrix bc;
        public Matrix DB;
        public Matrix T;

        public Structure(List<Node> _nodes, List<ShellElement> _elements, List<BC> _bcs, List<Load> _loads, List<DistributedLoad> _distLoads)
        {
            nodes = _nodes;
            elements = _elements;
            bcs = _bcs;
            loads = _loads;
            distLoads = _distLoads;
        }

        public Structure(List<Node> _nodes, List<ShellElement> _elements)
        {
            nodes = _nodes;
            elements = _elements;
        }

        public Structure()
        { }

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
            foreach (Load load in loads)
            {
                int[] loadDofs = new int[] {load.node.dofX, load.node.dofY, load.node.dofZ };

                f[loadDofs] = f[loadDofs] + load.LoadVec.ToVector();
            }

            // #TODO make it possible to have other bc's than fully fixed
            bc = Matrix.ZeroMatrix(bcs.Count * 6, 2);

            for (int i = 0; i < bcs.Count; i++)
            {
                bc[(i + 1) * 6 - 6, 0] = bcs[i].node.dofX;
                bc[(i + 1) * 6 - 6, 1] = 0;

                bc[(i + 1) * 6 - 5, 0] = bcs[i].node.dofY;
                bc[(i + 1) * 6 - 5, 1] = 0;

                bc[(i + 1) * 6 - 4, 0] = bcs[i].node.dofZ;
                bc[(i + 1) * 6 - 4, 1] = 0;

                bc[(i + 1) * 6 - 3, 0] = bcs[i].node.dofXX;
                bc[(i + 1) * 6 - 3, 1] = 0;

                bc[(i + 1) * 6 - 2, 0] = bcs[i].node.dofYY;
                bc[(i + 1) * 6 - 2, 1] = 0;

                bc[(i + 1) * 6 - 1, 0] = bcs[i].node.dofZZ;
                bc[(i + 1) * 6 - 1, 1] = 0;
            }

        }

        public bool Analyze(out Vector a, out Vector r, bool useExactMethod = false)
        {
            AssembleKfbc();


            return StaticFunctions.solveq(K, f, bc, out a, out r, useExactMethod);


        }

        public void CalcStresses(List<double> a, out List<Vector3D> principalStresses)
        {
            //Folllowing plants in MATlab
            //each element 15 dofs
            int[] activeDofs = new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16 };
            List<double> vMstresses = new List<double>();
            Matrix ed = new Matrix(elements.Count, 15);
            Matrix edDebug = new Matrix(elements.Count, 15);
            principalStresses = new List<Vector3D>();

            for (int i =0; i<elements.Count; i++)
            {
                int[] dofsFull = elements[i].GetElementDofs();

                //Transform a with local T matrix
                Matrix aTrans = new Matrix(18, 1);
                Matrix aDEBUG = new Matrix(18, 1);
                for (int j = 0; j < dofsFull.Count(); j++)
                    aTrans[j] = a[dofsFull[j]];

                 aDEBUG = aTrans;
                int tttette = 17 * (i + 1) + i;
                Matrix tTest = T[SF.intSrs(i * 18, 17 * (i + 1) + i), SF.intSrs(0, 17)];
                
                aTrans = T[SF.intSrs(i * 18, 17 * (i + 1) + i), SF.intSrs(0, 17)].Invert()* aTrans;
                
                 
                int[] dofs = new int[activeDofs.Count()];
                //Fixa hehe
                for (int j = 0; j < activeDofs.Count(); j++)
                    dofs[j] = dofsFull[activeDofs[j]];

                //Detta kan förbättras
                for (int j =0; j< activeDofs.Count();j++)
                {
                    ed[i, j] = aTrans[activeDofs[j]];
                    edDebug[i, j] = aDEBUG[activeDofs[j]];
                }
                    

                //fixa lite
                Matrix test = DB[SF.intSrs(i * 6, 5 * (i + 1) + i), SF.intSrs(0, 14)];
                Matrix ss = DB[SF.intSrs(i * 6, 5 * (i + 1) + i), SF.intSrs(0, 14)] * ed[i, SF.intSrs(0, 14)].Transpose();
                //ss[i, SF.intSrs(0, 5)] = ssTranspose.Transpose();

                //Von mises
             //   double vM = Math.Sqrt(Math.Pow(ss[i,0],2)+ Math.Pow(ss[i,1], 2)-ss[i,0]*ss[i,1]+3*Math.Pow(ss[i,2],2));
              //  vMstresses.Add(vM);

                //Principle stresses
                double p1 = (ss[0] + ss[1]) / 2.0 + Math.Sqrt(Math.Pow((ss[0]-ss[1])/2,2)+Math.Pow(ss[2],2));
                double p2 = (ss[0] + ss[1]) / 2.0 - Math.Sqrt(Math.Pow((ss[0] - ss[1]) / 2, 2) + Math.Pow(ss[2], 2));
                principalStresses.Add(new Vector3D(p1, p2, 0));


            }

        }
    }
}
