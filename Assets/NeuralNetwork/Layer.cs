using System;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Profiling;

namespace MonoRL
{
    [Serializable]
    public class Layer
    {
        [SerializeField]
        public int InputSize;//size
        [SerializeField]
        public int NodeSize;//size

        public List<Weights> Weights;//data
        public double[] Biases;
        public IActivation Activation;

        [SerializeField]
        private double[] _Inputs;//data
        [SerializeField]
        private double[] _Outputs;

        [NonSerialized]
        private double[][] _GradW;
        [NonSerialized]
        private double[] _GradB;

        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new List<Weights>();
            Biases = new double[nodeSize];
            _Inputs = new double[inputSize];
            _Outputs = new double[nodeSize];
            _GradB = new double[nodeSize];
            
            _GradW = new double[nodeSize][];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                _GradW[nodeIndex] = new double[InputSize];
            

            InitializeWeights();
            InitializeBiases();
        }
        public void SetNonSerializedData(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            Debug.Log("Update network");
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            _GradB = new double[nodeSize];
            _GradW = new double[nodeSize][];
            for (int nodeIndex = 0; nodeIndex < nodeSize; nodeIndex++)
                _GradW[nodeIndex] = new double[inputSize];
        }
        public double[] Forward(double[] inputs)
        {
            double[] activatedValues = new double[NodeSize];
            
            double calcOutput = 0;
            Weights weights;
            for (int nodeIndex = 0,inputIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                calcOutput = 0;
                inputIndex = 0;
                weights = Weights[nodeIndex];
                for (; inputIndex < InputSize; inputIndex++)
                {
                    calcOutput += weights.weigths[inputIndex] * inputs[inputIndex];
                }
                calcOutput += Biases[nodeIndex];
                _Outputs[nodeIndex] = calcOutput;
                
                activatedValues[nodeIndex] = Activation.Activate(calcOutput);
            }

            inputs.CopyTo(_Inputs, 0);

            return activatedValues;
        }

        public double[] Backward(double[] deltas)
        {
            double[] delta = new double[NodeSize];

            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                delta[nodeIndex] = deltas[nodeIndex] * Activation.Derivative(_Outputs[nodeIndex]);

            UpdateGradients(delta);
            double[] propagatedDelta = new double[InputSize];


            double delta_m = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                Weights n_weights = Weights[nodeIndex];
                delta_m = delta[nodeIndex];
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    propagatedDelta[inputIndex] += delta_m * n_weights.weigths[inputIndex];
                }
            }

            return propagatedDelta;
        }

        public void ApplyGradients(double lr, int batchSize)
        {
            Weights weights;
            double gradB, weightCalc = 0, gradW = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                gradB = _GradB[nodeIndex] / batchSize;
                Biases[nodeIndex] -= lr * gradB;
                weights = Weights[nodeIndex];
                
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    gradW = _GradW[nodeIndex][inputIndex] / batchSize;
                    
                    weightCalc = weights.weigths[inputIndex];
                    weightCalc -= lr * gradW;
                    weights.weigths[inputIndex] = weightCalc;
                }
            }

            ClearGradients();
        }

        public void ClearGradients()
        {
            Array.Clear(_GradB, 0, _GradB.Length);
            for (int i = 0; i < NodeSize; i++)
            {
                for (int j = 0; j < InputSize; j++)
                {
                    _GradW[i][j] = 0;
                }
            }
            //Array.Clear(_GradW, 0, _GradW.Length);
        }

        private void UpdateGradients(double[] delta)
        {
            double c_delta = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                c_delta = delta[nodeIndex];
                _GradB[nodeIndex] += c_delta;
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    _GradW[nodeIndex][inputIndex] += c_delta * _Inputs[inputIndex];
                }
            }
        }



        private void InitializeWeights()
        {
            float variance = 1.0f / NodeSize;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                Weights.Add(new Weights());
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                    Weights[nodeIndex].weigths.Add(UnityEngine.Random.Range(-Mathf.Sqrt(variance), Mathf.Sqrt(variance)));
            }
        }

        private void InitializeBiases()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                Biases[nodeIndex] = 0;
        }
    }

}
[Serializable]
public class Weights
{
    public List<double> weigths;
    public Weights()
    {
        weigths = new List<double>();
    }
}