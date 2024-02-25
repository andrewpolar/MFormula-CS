using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MFormula
{
    class DataGenerator
    {
        Random _rnd = new Random();

        public double Formula(double[] x)
        {
            //y = (1/pi)*(2+2*x3)*(1/3)*(atan(20*exp(x5)*(x1-0.5+x2/6))+pi/2) + (1/pi)*(2+2*x4)*(1/3)*(atan(20*exp(x5)*(x1-0.5-x2/6))+pi/2);
            double pi = 3.14159265359;
            if (5 != x.Length)
            {
                Console.WriteLine("Formala error");
                Environment.Exit(0);
            }
            double y = (1.0 / pi);
            y *= (2.0 + 2.0 * x[2]);
            y *= (1.0 / 3.0);
            y *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 + x[1] / 6.0)) + pi / 2.0;

            double z = (1.0 / pi);
            z *= (2.0 + 2.0 * x[3]);
            z *= (1.0 / 3.0);
            z *= Math.Atan(20.0 * Math.Exp(x[4]) * (x[0] - 0.5 - x[1] / 6.0)) + pi / 2.0;

            return y + z;
        }

        public double[] GetRandomInput()
        {
            double[] x = new double[5];
            x[0] = (_rnd.Next() % 1000) / 1000.0;
            x[1] = (_rnd.Next() % 1000) / 1000.0;
            x[2] = (_rnd.Next() % 1000) / 1000.0;
            x[3] = (_rnd.Next() % 1000) / 1000.0;
            x[4] = (_rnd.Next() % 1000) / 1000.0;
            return x;
        }
    }

    class DataHolder
    {
        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        public double[] _xmin = null;
        public double[] _xmax = null;
        private DataGenerator dg = new DataGenerator();

        public double[] GetRandomInput()
        {
            return dg.GetRandomInput();
        }

        public double GetTarget(double[] input)
        {
            return dg.Formula(input);
        }

        public void BuildFormulaData(int N)
        {
            _inputs.Clear();
            _target.Clear();

            double mean = 0.0;
            for (int i = 0; i < N; ++i)
            {
                double[] x = GetRandomInput();
                double y = dg.Formula(x);

                _inputs.Add(x);
                _target.Add(y);
                mean += y;
            }

            Console.WriteLine("Data is generated");
        }
    }
}
