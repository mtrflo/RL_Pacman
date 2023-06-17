using System;
using System.Collections;
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
using UnityEditor.PackageManager.Requests;
using System.Numerics;

namespace MonoRL
{
    [Serializable]
    public class Layer
    {
        [SerializeField]
        public int InputSize;//size
        [SerializeField]
        public int NodeSize;//size

        public float[] Weights;//data
        public float[] Biases;
        public IActivation Activation;

        [SerializeField]
        private float[] _Inputs;//data
        [SerializeField]
        private float[] _Outputs;

        public ComputeShader forwardCS, applyGradsCS;

        [NonSerialized]
        private float[] _GradW;
        [NonSerialized]
        private float[] _GradB;
        public Layer(int inputSize, int nodeSize, Activation.ActivationType activationType, ComputeShader forwardCS, ComputeShader applyGradsCS)
        {
            InputSize = inputSize;
            NodeSize = nodeSize;
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            Weights = new float[nodeSize * inputSize];
            Biases = new float[nodeSize];
            _Inputs = new float[inputSize];
            _Outputs = new float[nodeSize];
            _GradB = new float[nodeSize];

            _GradW = new float[nodeSize * inputSize];

            this.forwardCS = forwardCS;
            this.applyGradsCS = applyGradsCS;
            InitializeWeights();
            InitializeBiases();
            Awake();
        }
        public void SetNonSerializedData(int inputSize, int nodeSize, Activation.ActivationType activationType)
        {
            Debug.Log("Update network");
            Activation = MonoRL.Activation.GetActivationFromType(activationType);
            _GradB = new float[nodeSize];
            _GradW = new float[nodeSize * inputSize];

            Awake();
        }

        ComputeBuffer outputBuffer;
        ComputeBuffer weightBuffer;
        ComputeBuffer biaseBuffer;
        ComputeBuffer _GradB_buffer;
        ComputeBuffer _GradW_buffer;
        public NativeArray<float> na_inputs, na_Weights, na_Biases, na__Outputs, na_activatedValues;
        ForwardBurst forwardBurst;
        static JobHandle lastJobHandle;
        void Awake()
        {
            floatsize = sizeof(float);

            outputBuffer = new ComputeBuffer(_Outputs.Length, floatsize);//outputs
            weightBuffer = new ComputeBuffer(Weights.Length, floatsize);//weights
            biaseBuffer = new ComputeBuffer(Biases.Length, floatsize);//biases
            _GradB_buffer = new ComputeBuffer(_GradB.Length, floatsize);
            _GradW_buffer = new ComputeBuffer(_GradW.Length, floatsize);
            //Allocator alc = Allocator.Persistent;
            //na_inputs = new NativeArray<float>(InputSize, alc);
            //na_Weights = new NativeArray<float>(Weights.Length, alc);
            //na_Biases = new NativeArray<float>(Biases.Length, alc);
            //na__Outputs = new NativeArray<float>(_Outputs.Length, alc);
            //na_activatedValues = new NativeArray<float>(NodeSize, alc);
            
            //forwardBurst = new ForwardBurst();
            //forwardBurst.NodeSize = NodeSize;
            //forwardBurst.InputSize = InputSize;
            //forwardBurst.activatedValues = na_activatedValues;
            //forwardBurst._Outputs = na__Outputs;
            //forwardBurst.Weights = na_Weights;
            //forwardBurst.inputs = na_inputs;
            //forwardBurst.Biases = na_Biases;
        }
        //
        public float[] Forward(float[] inputs)
        {
            //return ForwardBurst(inputs);
            float[] activatedValues = new float[NodeSize];
            
            float calcOutput = 0;
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
        //
        public float[] ForwardBurst(float[] inputs)
        {
            float[] activatedValues;

            na_inputs.CopyFrom(inputs);
            na_Weights.CopyFrom(Weights);
            na_Biases.CopyFrom(Biases);


            JobHandle jobHandle = forwardBurst.Schedule(NodeSize, 100);
            
            jobHandle.Complete();

            activatedValues = na_activatedValues.ToArray();
            _Outputs = na__Outputs.ToArray();
            inputs.CopyTo(_Inputs, 0);
            return activatedValues;
        }
        int floatsize;
        //
        public System.Collections.IEnumerator ForwardGPU(float[] inputs, Action<float[]> complete)
        {
            //Debug.Log("aa");
            float[] activatedValues = new float[NodeSize];
            ComputeBuffer activatedValueBuffer = new ComputeBuffer(activatedValues.Length, floatsize);//activated values
            ComputeBuffer inputBuffer = new ComputeBuffer(inputs.Length, floatsize);//inputs



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

            //outputBuffer.GetData(_Outputs);
            var request = AsyncGPUReadback.Request(outputBuffer, 0, 0);
            yield return new WaitUntil(() => request.done);
            NativeArray<float> na = request.GetData<float>();
            na.CopyTo(_Outputs);
            complete.Invoke(_Outputs);


            inputs.CopyTo(_Inputs, 0);

            //return activatedValues;
        }
        //
        public float[] Backward(float[] deltas)
        {
            float[] delta = new float[NodeSize];

            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
                delta[nodeIndex] = deltas[nodeIndex] * Activation.Derivative(_Outputs[nodeIndex]);

            UpdateGradients(delta);
            float[] propagatedDelta = new float[InputSize];


            float delta_m = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                delta_m = delta[nodeIndex];
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    propagatedDelta[inputIndex] += delta_m * Weights[nodeIndex * InputSize + inputIndex];
                }
            }
            //
            return propagatedDelta;
        }

        public void ApplyGradients(float lr, int batchSize)
        {
            float gradB, weightCalc = 0, gradW = 0;
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

        public IEnumerator ApplyGradientsGPU(float lr, int batchSize)
        {
            //Debug.Log("aa");
            float[] activatedValues = new float[NodeSize];
            



            //activatedValueBuffer.SetData(activatedValues);
            //outputBuffer.SetData(_Outputs);
            biaseBuffer.SetData(Biases);
            weightBuffer.SetData(Weights);
            _GradB_buffer.SetData(_GradB);
            _GradW_buffer.SetData(_GradW);

            applyGradsCS.SetBuffer(0, "Biases", biaseBuffer);
            applyGradsCS.SetBuffer(0, "Weights", weightBuffer);
            applyGradsCS.SetBuffer(0, "_GradB", _GradB_buffer);
            applyGradsCS.SetBuffer(0, "_GradW", _GradW_buffer);
            applyGradsCS.SetInt("InputSize", InputSize);
            applyGradsCS.SetInt("batchSize", batchSize);
            applyGradsCS.SetFloat("lr", lr);
            
            applyGradsCS.Dispatch(0, NodeSize < 10 ? 1 : (NodeSize / 10), 1, 1);
            
            var requestb = AsyncGPUReadback.Request(biaseBuffer);
            yield return new WaitUntil(() => requestb.done);
            NativeArray<float> nab = requestb.GetData<float>();
            nab.CopyTo(Biases);

            var requestw = AsyncGPUReadback.Request(weightBuffer);
            yield return new WaitUntil(() => requestw.done);
            NativeArray<float> naw = requestw.GetData<float>();
            naw.CopyTo(Weights);

            naw.Dispose();
            nab.Dispose();
            ClearGradients();
        }

        public void ClearGradients()
        {
            Array.Clear(_GradB, 0, _GradB.Length);
            Array.Clear(_GradW, 0, _GradW.Length);
        }

        private void UpdateGradients(float[] delta)
        {
            float c_delta = 0;
            for (int nodeIndex = 0; nodeIndex < NodeSize; nodeIndex++)
            {
                c_delta = delta[nodeIndex];
                _GradB[nodeIndex] += c_delta;
                for (int inputIndex = 0; inputIndex < InputSize; inputIndex++)
                {
                    _GradW[nodeIndex * InputSize + inputIndex] += c_delta * _Inputs[inputIndex];
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

    public NativeArray<float> activatedValues, _Outputs;
    [ReadOnly]
    public NativeArray<float> inputs, Weights, Biases;
    const float a = 0.01f;
    public float Activate(float z)
    {
        return (z >= 0) ? z : a * z;
    }
    public void Execute(int i)
    {
        float calcOutput = 0;
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