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

namespace MonoRL
{
    [Serializable]
    public class Layer
    {
        [SerializeField]
        public int InputSize;//size
        [SerializeField]
        public int NodeSize;//size

        public double[] Weights;//data
        public double[] Biases;
        public IActivation Activation;

        [SerializeField]
        private double[] _Inputs;//data
        [SerializeField]
        private double[] _Outputs;

        public ComputeShader forwardCS;

        [NonSerialized]
        private double[][] _GradW;
        [NonSerialized]
        private double[] _GradB;

        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType, ComputeShader forwardCS)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new double[nodeSize * inputSize];
            Biases = new double[nodeSize];
            _Inputs = new double[inputSize];
            _Outputs = new double[nodeSize];
            _GradB = new double[nodeSize];

            _GradW = new double[nodeSize][];
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                _GradW[nodeIndex] = new double[InputSize];

            this.forwardCS = forwardCS;

            InitializeWeights();
            InitializeBiases();
            Awake();
        }
        public void SetNonSerializedData(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            Debug.Log("Update network");
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            _GradB = new double[nodeSize];
            _GradW = new double[nodeSize][];
            for (int nodeIndex = 0; nodeIndex < nodeSize; nodeIndex++)
                _GradW[nodeIndex] = new double[inputSize];

            Awake();
        }

        ComputeBuffer outputBuffer;
        ComputeBuffer weightBuffer;
        ComputeBuffer biaseBuffer;

        public NativeArray<double> na_inputs, na_Weights, na_Biases, na__Outputs, na_activatedValues;
        ForwardBurst forwardBurst;
        static JobHandle lastJobHandle;
        void Awake()
        {
            doublesize = sizeof(double);

            outputBuffer = new ComputeBuffer(_Outputs.Length, doublesize);//outputs
            weightBuffer = new ComputeBuffer(Weights.Length, doublesize);//weights
            biaseBuffer = new ComputeBuffer(Biases.Length, doublesize);//biases

            Allocator alc = Allocator.Persistent;
            na_inputs = new NativeArray<double>(InputSize, alc);
            na_Weights = new NativeArray<double>(Weights.Length, alc);
            na_Biases = new NativeArray<double>(Biases.Length, alc);
            na__Outputs = new NativeArray<double>(_Outputs.Length, alc);
            na_activatedValues = new NativeArray<double>(NodeSize, alc);
            
            forwardBurst = new ForwardBurst();
            forwardBurst.NodeSize = NodeSize;
            forwardBurst.InputSize = InputSize;
            forwardBurst.activatedValues = na_activatedValues;
            forwardBurst._Outputs = na__Outputs;
            forwardBurst.Weights = na_Weights;
            forwardBurst.inputs = na_inputs;
            forwardBurst.Biases = na_Biases;

        }

        public double[] Forward(double[] inputs)
        {
            return ForwardBurst(inputs);

            double[] activatedValues = new double[NodeSize];


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

            inputs.CopyTo(_Inputs, 0);

            return activatedValues;
        }
        public double[] ForwardBurst(double[] inputs)
        {
            double[] activatedValues;

            na_inputs.CopyFrom(inputs);
            na_Weights.CopyFrom(Weights);
            na_Biases.CopyFrom(Biases);


            JobHandle jobHandle = forwardBurst.Schedule(NodeSize, 128 * 128);
            jobHandle.Complete();

            activatedValues = na_activatedValues.ToArray();
            _Outputs = na__Outputs.ToArray();
            inputs.CopyTo(_Inputs, 0);
            return activatedValues;
        }
        int doublesize;
        public double[] ForwardGPU(double[] inputs)
        {

            //Debug.Log("aa");
            double[] activatedValues = new double[NodeSize];
            ComputeBuffer activatedValueBuffer = new ComputeBuffer(activatedValues.Length, doublesize);//activated values
            ComputeBuffer inputBuffer = new ComputeBuffer(inputs.Length, doublesize);//inputs



            //activatedValueBuffer.SetData(activatedValues);
            //outputBuffer.SetData(_Outputs);
            weightBuffer.SetData(Weights);
            inputBuffer.SetData(inputs);
            biaseBuffer.SetData(Biases);

            forwardCS.SetBuffer(0, "outputs", outputBuffer);
            forwardCS.SetBuffer(0, "activatedValues", activatedValueBuffer);
            forwardCS.SetBuffer(0, "Weights", weightBuffer);
            forwardCS.SetBuffer(0, "inputs", inputBuffer);
            forwardCS.SetBuffer(0, "Biases", biaseBuffer);
            forwardCS.SetInt("InputSize", InputSize);

            forwardCS.Dispatch(0, NodeSize < 10 ? 1 : (NodeSize / 10), 1, 1);
            activatedValueBuffer.GetData(activatedValues);

            outputBuffer.GetData(_Outputs);
            //AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(outputBuffer, 0, 0);
            //request.
            //request.completed += op =>
            //{
            //    if (op.status == AsyncGPUReadbackStatus.Success)
            //    {
            //        result = op.GetData<float>().ToArray();
            //        Debug.Log("Async GPU readback completed successfully.");
            //    }
            //    else
            //    {
            //        Debug.LogError("Async GPU readback failed with error: " + op.status);
            //    }
            //};
            //activatedValueBuffer.Release();
            //outputBuffer.Release();


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
                delta_m = delta[nodeIndex];
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    propagatedDelta[inputIndex] += delta_m * Weights[nodeIndex * InputSize + inputIndex];
                }
            }

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
                    gradW = _GradW[nodeIndex][inputIndex] / batchSize;

                    weightCalc = Weights[nodeIndex * InputSize + inputIndex];
                    weightCalc -= lr * gradW;
                    Weights[nodeIndex * InputSize + inputIndex] = weightCalc;
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
            float sqrtVar = Mathf.Sqrt(variance);
            for (int i = 0; i < NodeSize * InputSize; i++)
                Weights[i] = Random.Range(-sqrtVar, sqrtVar);
        }

        private void InitializeBiases()
        {
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                Biases[nodeIndex] = 0;
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