﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SF = MiStrAnEngine.StaticFunctions;
using mat = MiStrAnEngine.Materials;
using MiStrAnEngine;

namespace Sandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //// shell
            //Node node1 = new Node(0, 0, 0);
            //Node node2 = new Node(1, 0, 0);
            //Node node3 = new Node(0, 1, 0);

            //List<Node> nodeList = new List<Node>() { node1, node2, node3 };



            //ShellElement element = new ShellElement(nodeList, 1);
            ////element.thickness = 1;

            //element.thickness = 0.1;
            //element.SetSteelSection();

            //Matrix Ke, fe;

            //element.GenerateKefe(out Ke, out fe);

            ////Matrix test = new Matrix(3, 3);





            //////element.ShellTesting();

            //////element.GetLocalNodeCoordinates();
            //////element.TestingShell2();
            ////double E = 1;
            ////double v = 0.3;
            ////double G = E / (2.0 * (1 + v));
            ////double[] angle = new double[] { 0, 0, 0, 0, 0, 0, 0 };
            ////element.D = Materials.eqModulus(E, E, G, v, angle, 1.0 / 7.0);

            ////// element.D =// new Matrix(new double[,] { { 1.3462, 0.5769, 0.5769, 0,0,0 },
            /////* { 0.5769, 1.3462, 0.5769,0,0,0 },
            ////                                         { 0.5769, 0.5769, 1.3462,0,0,0 },
            ////                                         { 0, 0, 0,0.3846,0,0 },
            ////                                         { 0, 0, 0,0,0.3846,0 },
            ////                                         { 0, 0, 0,0,0,0.3846 } });*/


            ////element.GenerateKefe(out Ke, out fe);

            ////element.GetLocalNodeCoordinates();
            ////element.TestingShell2();
            ///*  double E = 210*Math.Pow(10,9);
            //  double v = 0.3;
            //  double G = E / (2.0 * (1 + v));
            //  double[] angle = new double[] { 0, 0, 0, 0, 0, 0, 0 };
            //  element.D = Materials.eqModulus(E, E, G, v, angle, 1.0 / 7.0);

            //  // element.D =// new Matrix(new double[,] { { 1.3462, 0.5769, 0.5769, 0,0,0 },
            //  /* { 0.5769, 1.3462, 0.5769,0,0,0 },
            //                                           { 0.5769, 0.5769, 1.3462,0,0,0 },
            //                                           { 0, 0, 0,0.3846,0,0 },
            //                                           { 0, 0, 0,0,0.3846,0 },
            //                                           { 0, 0, 0,0,0,0.3846 } });*/

            //element.thickness = 0.1;
            //element.SetSteelSection();
            //element.GenerateKefe(out Ke, out fe);

            //Matrix Testa = new Matrix(new double[,] { { 1, 2, 3 ,4}, { 5, 6, 7,8 }, { 9, 10, 11,12 }, { 13,14,15,16} });
            //int[] dofsTEST =new int[3]{2 ,0,1};
            //Matrix Test2 = Testa[dofsTEST, dofsTEST];

            //Test2 = Test2 + Test2;
            //Testa[dofsTEST, dofsTEST] = Testa[dofsTEST, dofsTEST] + Testa[dofsTEST, dofsTEST];
            ////element.t = 2;
            ////element.eq = new Matrix(new double[,] { { 5,5,5} });


            ////Matrix Ke;
            ////Matrix fe;

            ////element.GenerateKefe(out Ke, out fe);

            ////Ke = Ke;

            ////Solveq test case

            ////Matrix K = new Matrix(new double[,] { { 2, -1 }, { -1, 2 } });
            ////Matrix f = new Matrix(new double[,] { { 0}, { 10 } });
            ////Matrix bc = new Matrix(new double[,] { { 0 , 0 } });

            ////Matrix d, Q;

            ////SF.solveq(K, f, bc, out d, out Q);

            //int[] dofs = new int[] { 0, 1, 6, 7, 12, 13 };

            //Matrix KTest = Ke[dofs, dofs];
            //Matrix f = fe[dofs, 0];

            Matrix A = new Matrix(3, 3);
            Matrix B = new Matrix(3, 3);
            Matrix C = new Matrix(3, 3);

            A[0, 0] = 1;
            A[0, 2] = 3;
            A[1, 0] = 5;
            A[1, 1] = 4;
            A[2, 2] = 1;

            B[0, 0] = 3;
            B[0, 1] = 5;
            B[1, 0] = 6;
            B[1, 2] = 7;
            B[2, 2] = 2;

            C[0, 0] = 1;
            C[0, 1] = 1;
            C[0, 2] = 1;
            C[1, 0] = 1;
            C[1, 1] = 1;
            C[2, 0] = 1;
            C[2, 2] = 1;

            double[] b = new double[3];
            b[0] = 6;
            b[1] = 3;
            b[2] = 4;



            Matrix test = StaticFunctions.SolveWith_CG_alglib(C.ToAlglibSparse(), b);


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            double E1 = 33.18*1000000000; 
            double E2 = 7.74* 1000000000; //GPA
            double G12 = 2.91* 1000000000; //GPa
            double v12 = 0.267;
            double[] angle = new double[4] { 45,30, 30, 45};
            double laminaThickness = 0.004; //mm
            double density = 2100;

            Matrix Ltest = new Matrix(new double[,] { { 0.3, 0.4, 0.5 } });
            Matrix xetest = new Matrix(new double[,] { { 2,3,0 }, { -1, 2, 0 }, { 4, 2, 0 } });
            Matrix Btest;
            Matrix Ntest;

            MiStrAnEngine.ShellElement testS = new MiStrAnEngine.ShellElement(new List<Node>(),new int());
            testS.GetB_N(Ltest, xetest, out Btest, out Ntest);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Matrix K = new Matrix(new double[,] { { 2, -1, 0 }, { -1, 2, -1 }, { 0, -1, 2 } });

            double[,] blaha = new double[,] {
           {     1   ,  0  ,   0 ,- 1   ,  0   ,  0 },
   {  0  ,  12  ,   6   ,  0, - 12 ,    6 },
    { 0  ,   6   ,  4  ,   0, - 6,     2 },
  { -1  ,   0  ,   0  ,   1  ,   0   ,  0 },
   {  0, -12, -6,     0,    12, -6 },
   {  0  ,   6   ,  2  ,   0, - 6 ,    4 } };
            K = new Matrix(blaha);
            Matrix bc = new Matrix(3, 2);
            bc[1, 0] = 1;
            bc[2, 0] = 2;

            Matrix f = new Matrix(6, 1);
            f[4] = 1;

            Matrix a, r;

            SF.solveq(K, f, bc, out a, out r);

            a = a;




        }
    }
}
