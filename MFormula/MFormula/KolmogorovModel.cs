using System;
using System.Collections.Generic;
using System.Text;

namespace MFormula
{
    class KolmogorovModel
    {
        //Model parameters 
        int points_in_interior = 16;
        int points_in_exterior = 32;
        double muRoot = 0.01;
        double muLeaves = 0.01;
        int nEpochs = 100;
        int nLeaves = -1; //negative number means it is chosen according to theory
        //////////////////////////////////////////////////////////////////////////

        public List<double[]> _inputs = new List<double[]>();
        public List<double> _target = new List<double>();
        private double[] _xmin = null;
        private double[] _xmax = null;
        private double _targetMin;
        private double _targetMax;
        int[] _interior_structure = null;
        int[] _exterior_structure = null;

        private List<U> _ulist = new List<U>();
        private U _bigU = null;
        private Random _rnd = new Random();

        public KolmogorovModel(List<double[]> inputs, List<double> target)
        {
            _inputs = inputs;
            _target = target;

            if (inputs.Count != target.Count)
            {
                Console.WriteLine("Invalid training data");
                Environment.ExitCode = 0;
            }

            FindMinMax();

            int number_of_inputs = _inputs[0].Length;
            if (nLeaves < 0)
            {
                nLeaves = number_of_inputs * 2 + 1;
            }
            _interior_structure = new int[number_of_inputs];
            for (int i = 0; i < number_of_inputs; i++)
            {
                _interior_structure[i] = points_in_interior;
            }
            _exterior_structure = new int[nLeaves];
            for (int i = 0; i < nLeaves; i++)
            {
                _exterior_structure[i] = points_in_exterior;
            }

            GenerateInitialOperators();
        }

        private void FindMinMax()
        {
            int size = _inputs[0].Length;
            _xmin = new double[size];
            _xmax = new double[size];

            for (int i = 0; i < size; ++i)
            {
                _xmin[i] = double.MaxValue;
                _xmax[i] = double.MinValue;
            }

            for (int i = 0; i < _inputs.Count; ++i)
            {
                for (int j = 0; j < _inputs[i].Length; ++j)
                {
                    if (_inputs[i][j] < _xmin[j]) _xmin[j] = _inputs[i][j];
                    if (_inputs[i][j] > _xmax[j]) _xmax[j] = _inputs[i][j];
                }

            }

            _targetMin = double.MaxValue;
            _targetMax = double.MinValue;
            for (int j = 0; j < _target.Count; ++j)
            {
                if (_target[j] < _targetMin) _targetMin = _target[j];
                if (_target[j] > _targetMax) _targetMax = _target[j];
            }
        }

        public void GenerateInitialOperators()
        {
            _ulist.Clear();
            int points = _inputs[0].Length;
            for (int counter = 0; counter < nLeaves; ++counter)
            {
                U uc = new U(_xmin, _xmax, _targetMin, _targetMax, _interior_structure);
                _ulist.Add(uc);
            }

            if (null != _bigU)
            {
                _bigU.Clear();
                _bigU = null;
            }

            double[] min = new double[nLeaves];
            double[] max = new double[nLeaves];
            for (int i = 0; i < nLeaves; ++i)
            {
                min[i] = _targetMin;
                max[i] = _targetMax;
            }

            _bigU = new U(min, max, _targetMin, _targetMax, _exterior_structure);
        }

        private double[] GetVector(double[] data)
        {
            int size = _ulist.Count;
            double[] vector = new double[size];
            for (int i = 0; i < size; ++i)
            {
                vector[i] = _ulist[i].GetU(data);
            }
            return vector;
        }

        public void BuildRepresentation()
        {
            for (int step = 0; step < nEpochs; ++step)
            {
                double error = 0.0;
                for (int i = 0; i < _inputs.Count; ++i)
                {
                    double[] v = GetVector(_inputs[i]);
                    double model = _bigU.GetU(v);
                    double diff = _target[i] - model;
                    error += diff * diff;

                    for (int k = 0; k < _ulist.Count; ++k)
                    {
                        if (v[k] > _targetMin && v[k] < _targetMax)
                        {
                            double derrivative = _bigU.GetDerrivative(k, v[k]);
                            _ulist[k].Update(diff * derrivative / v.Length, _inputs[i], muLeaves);
                        }
                    }
                    _bigU.Update(diff, v, muRoot);
                }
                error /= _inputs.Count; 
                error = Math.Sqrt(error);
                error /= (_targetMax - _targetMin); 
                Console.WriteLine("Training step {0}, RMSE {1:0.0000}", step, error);
            }
        }

        public double ComputeOutput(double[] inputs)
        {
            double[] v = GetVector(inputs);
            double output = _bigU.GetU(v);
            return output;
        }
    }
}
