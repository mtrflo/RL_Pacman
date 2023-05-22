using System;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using Unity.Jobs;
using Unity.Burst;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.Collections;
using Unity.Mathematics;

namespace MonoRL
{
    [BurstCompile][Serializable]
    public struct Layer : IJob
    {
        [SerializeField]
        public int InputSize;//size
        [SerializeField]
        public int NodeSize;//size

        public NativeArray<double> Weights;//data
        public NativeArray<double> Biases;
        public IActivation Activation;

        [SerializeField]
        public NativeArray<double> _Inputs;//data
        [SerializeField]
        public NativeArray<double> _Outputs;

        [NonSerialized]
        public NativeArray<double> _GradW;
        [NonSerialized]
        public NativeArray<double> _GradB;
        Allocator allo;
        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            allo = Allocator.Persistent;
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);

            Weights = new NativeArray<double>(nodeSize * inputSize, allo);
            Biases = new NativeArray<double>(nodeSize, allo);
            _Inputs = new NativeArray<double>(inputSize, allo);
            _Outputs = new NativeArray<double>(nodeSize, allo);
            _GradB = new NativeArray<double>(nodeSize, allo);

            _GradW = new NativeArray<double>(nodeSize * inputSize, allo);

            InitializeWeights();
            InitializeBiases();
            Awake();
        }
        public void SetNonSerializedData(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            allo = Allocator.Persistent;

            Debug.Log("Update network");
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            _GradB = new NativeArray<double>(nodeSize, allo);
            _GradW = new NativeArray<double>(nodeSize * inputSize, allo);

            Awake();
        }
        void Awake()
        {
            

        }

        public NativeArray<double> Forward(NativeArray<double> inputs)
        {
            NativeArray<double> activatedValues = new NativeArray<double>(NodeSize, allo);
            double calcOutput = 0;
            for (int nodeIndex = 0, inputIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                calcOutput = 0;
                inputIndex = 0;
                for (; inputIndex < InputSize; inputIndex++)
                {
                    calcOutput += Weights[nodeIndex * InputSize + inputIndex] * inputs[inputIndex];
                }
                calcOutput += Biases[nodeIndex];
                _Outputs[nodeIndex] = calcOutput;

                activatedValues[nodeIndex] = Activation.Activate(calcOutput);
            }

            inputs.CopyTo(_Inputs);
            inputs.Dispose();
            return activatedValues;
        }
        public NativeArray<double> Backward(NativeArray<double> deltas)
        {
            NativeArray<double> delta = new NativeArray<double>(NodeSize,Allocator.TempJob), delta2 = new NativeArray<double>(NodeSize, Allocator.TempJob);
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                delta[nodeIndex] = deltas[nodeIndex] * Activation.Derivative(_Outputs[nodeIndex]);
            delta.CopyTo(delta2);
            deltas.Dispose();

            UpdateGradients(new NativeArray<double>(delta, Allocator.TempJob));
            NativeArray<double> propagatedDelta = new NativeArray<double>(InputSize,allo);


            double delta_m = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                delta_m = delta2[nodeIndex];
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    propagatedDelta[inputIndex] += delta_m * Weights[nodeIndex * InputSize + inputIndex];
                }
            }
            delta.Dispose();
            delta2.Dispose();
            return propagatedDelta;
        }

        public void ApplyGradients(double lr, int batchSize)
        {
            double gradB, weightCalc = 0, gradW = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                gradB = _GradB[nodeIndex] / batchSize;
                Biases[nodeIndex] -= lr * gradB;

                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    gradW = _GradW[nodeIndex * InputSize + inputIndex] / batchSize;

                    weightCalc = Weights[nodeIndex * InputSize + inputIndex];
                    weightCalc -= lr * gradW;
                    Weights[nodeIndex * InputSize + inputIndex] = weightCalc;
                }
            }

            ClearGradients();
        }

        public void ClearGradients()
        {
            _GradB.Dispose();
            _GradW.Dispose();
            _GradB = new NativeArray<double>(NodeSize, allo);
            _GradW = new NativeArray<double>(NodeSize * InputSize, allo);
        }

        private void UpdateGradients(NativeArray<double> delta)
        {
            double c_delta = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                c_delta = delta[nodeIndex];
                _GradB[nodeIndex] += c_delta;
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    _GradW[nodeIndex * InputSize + inputIndex] += c_delta * _Inputs[inputIndex];
                }
            }
            delta.Dispose();
        }



        private void InitializeWeights()
        {
            float variance = 1.0f / NodeSize;
            
            float sqrtVar = math.sqrt(variance);
            for (int i = 0; i < NodeSize * InputSize; i++)
                Weights[i] = Random.Range(-sqrtVar, sqrtVar);
        }

        private void InitializeBiases()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                Biases[nodeIndex] = 0;
        }

        public void Execute()
        {
            
        }
    }

}
[BurstCompile]
public struct ForwardBurst : IJobParallelFor//a
{
    public int NodeSize, InputSize;

    public NativeArray<double> activatedValues, _Outputs;
    [ReadOnly]
    public NativeArray<double> inputs, Weights, Biases;
    const double a = 0.01;
    public double Activate(double z)
    {
        return (z >= 0) ? z : a * z;
    }
    public void Execute(int i)
    {
        double calcOutput = 0;
        int nodeIndex = i, inputIndex = 0;

        calcOutput = 0;
        inputIndex = 0;
        for (; inputIndex < InputSize; inputIndex++)
        {
            calcOutput += Weights[nodeIndex * InputSize + inputIndex] * inputs[inputIndex];
        }
        calcOutput += Biases[nodeIndex];
        _Outputs[nodeIndex] = calcOutput;

        activatedValues[nodeIndex] = Activate(calcOutput);

    }
}