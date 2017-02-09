﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiStrAnEngine
{
    public class Vector
    {
        public double X, Y, Z;

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector() : this(0, 0, 0) { }


        public Vector(Vector copy) : this(copy.X, copy.Y, copy.Z) { }

        public Vector Normalize(bool overwrite = true)
        {
            double L = this.Length;
            Vector v = new Vector();

            v.X = this.X / L;
            v.Y = this.Y / L;
            v.Z = this.Z / L;

            if (overwrite)
            {
                this.X = v.X;
                this.Y = v.Y;
                this.Z = v.Z;
            }

            return v;
        }

        public Matrix ToMatrix()
        {
            return new Matrix(new double[,] { { this.X }, { this.Y }, { this.Z }, });
        }

        public static Vector operator +(Vector a, Vector b)
        { return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        
        public static Vector operator -(Vector a, Vector b)
        { return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

        public static Vector operator *(double a, Vector b)
        { return new Vector(a * b.X, a * b.Y, a * b.Z);   }

        public static Vector operator *(Vector a, double b)
        { return b * a; }

        public static Vector operator -(Vector a)
        { return new Vector(-a.X, -a.Y, -a.Z); }

        public static double operator*(Vector a, Vector b)
        { return DotProduct(a, b); }

        public static Vector operator /(Vector a, double b)
        { return a*(1/b); }

        public static double DotProduct(Vector a, Vector b)
        { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }

        public static Vector CrossProduct(Vector a, Vector b)
        { return new Vector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }

        public double Length
        { get { return Math.Sqrt(Math.Pow(this.X, 2) + Math.Pow(this.Y, 2) + Math.Pow(this.Z, 2)); } }

        public static Vector e1
        {
            get { return new Vector(1, 0, 0); }
        }

        public static Vector e2
        {
            get { return new Vector(0, 1, 0); }
        }

        public static Vector e3
        {
            get { return new Vector(0, 0, 1); }
        }



    }
}
