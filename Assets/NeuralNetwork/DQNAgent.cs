using MonoRL;
using NaughtyAttributes;
using PMT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;


public class DQNAgent : MonoBehaviour
{
    public static DQNAgent me;
    public string version = "test";
    [Header("Cloning")]
    public TextAsset clonedNetwork;
    public bool clone;
    [Range(0, 1)]
    public float epsilon = 0.8f;//exploit - explore     0-1
    [Range(0, 1)]
    public float gamma = 0.8f;
    [Range(0, 1)]
    public float lr = 0.1f;
    
    #region ReplayBuffer
    [Header("ReplayBuffer")]
    [HideInInspector] public int bufferSize = 1;
    [HideInInspector] public int batchSize = 1;
    [HideInInspector] public int pCount = 1;
    [Range(0, 1)]
    [HideInInspector] public float softUpdateFactor = 1;
    [HideInInspector] public ReplayBuffer<Transition> replayBuffer;
    [HideInInspector] public Network targetNetwork;
    #endregion
    
    public Network network;
    private void Awake()
    {
        replayBuffer = new ReplayBuffer<Transition>(bufferSize);

        if (me != null)
        {
            Destroy(gameObject);
            return;
        }
        me = this;
        LoadNetwork();


        network.Init();

        targetNetwork = DuplicateNetwork(network);
        ReplaceTarget();
        //DontDestroyOnLoad(this);
    }
    public int SelectAction(float[] observation)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > epsilon)
        {
            float[] actionValues = network.Forward(observation);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, network.Layers.Last().NodeSize);

        return action;
    }

    public int SelectAction(float[] observation,float _epsilon)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > _epsilon)
        {
            float[] actionValues = network.Forward(observation);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, network.Layers.Last().NodeSize);

        return action;
    }

    public void Learn(Transition transition)
    {
        //replayBuffer.Add(transition);

        //if (replayBuffer.Size() >= batchSize)
        //{
            //for (int e = 0; e < pCount; e++)
            //{

                Transition[] randomSamples = new Transition[] { transition};

                float[][] batchInputs = randomSamples.Select(x => x.prev_state).ToArray();
                float[][] batchExpectedOutputs = new float[batchInputs.Length][];

                for (int batchIndex = 0; batchIndex < batchSize; batchIndex++)
                {
                    Transition sampleTransition = randomSamples[batchIndex];
                    batchInputs[batchIndex] = sampleTransition.prev_state;

                    float[] predictedValues = network.Forward(sampleTransition.prev_state);
                    float QEval = predictedValues[sampleTransition.action];
                    float QNext = network.Forward(sampleTransition.state_).Max();
                    float QTarget = sampleTransition.reward + gamma * QNext;
                    if (sampleTransition.isDone)
                        QTarget = sampleTransition.reward;

                    float[] expectedValues = predictedValues.ToArray();
                    
                    
                    expectedValues[sampleTransition.action] = QTarget;
                    
                    batchExpectedOutputs[batchIndex] = expectedValues;
                }

                network.Learn(batchInputs, batchExpectedOutputs);
            //}
        //}
    }

    public void LearnSupervised(Transition transition)
    {
        Transition[] randomSamples = new Transition[] { transition };
        float[][] batchInputs = randomSamples.Select(x => x.prev_state).ToArray();
        float[][] batchExpectedOutputs = new float[batchInputs.Length][];
        float[] expectedValues = new float[network.layersSize.Last()];
        for (int i = 0; i < expectedValues.Length; i++)
            expectedValues[i] = -1;
        expectedValues[transition.action] = 1;
        batchExpectedOutputs[0] = expectedValues;
        network.Learn(batchInputs, batchExpectedOutputs);

    }

    public void ReplaceTarget()
    {
        void CopyUpdate()
        {
            int mainNetworkLayerCount = network.Layers.Count;
            int mainNetworkLayerWeightCount = 0;
            for (int i = 0; i < mainNetworkLayerCount; i++)
            {
                Layer mainNetworkLayer = network.Layers[i],
                      targetNetworkLayer = targetNetwork.Layers[i];
                for (int j = 0; j < mainNetworkLayerWeightCount; j++)
                {
                    mainNetworkLayer.Weights.CopyTo(targetNetworkLayer.Weights, 0);
                    mainNetworkLayer.Biases.CopyTo(targetNetworkLayer.Biases, 0);
                }
            }
        }

        

        void SoftUpdate()
        {
            float updateFactor = softUpdateFactor;

            int mainNetworkLayerCount = network.Layers.Count
            , mainNetworkLayerWeightCount = 0
            , mainNetworkLayersWeightsWeigthsCount = 0
            , layerBiasCount = 0;

            
            for (int i = 0; i < mainNetworkLayerCount; i++)
            {
                mainNetworkLayerWeightCount = network.Layers[i].Weights.Length;
                Layer mainNetworkLayer = network.Layers[i],
                      targetNetworkLayer = targetNetwork.Layers[i];
                layerBiasCount = mainNetworkLayer.Biases.Length;
                for (int j = 0; j < mainNetworkLayerWeightCount; j++)
                {
                    for (int k = 0; k < mainNetworkLayersWeightsWeigthsCount; k++)
                        targetNetworkLayer.Weights[j] = math.lerp(targetNetworkLayer.Weights[j], mainNetworkLayer.Weights[j], updateFactor);
                }
                float[] mainNetworkBiases, targetNetworkBiases;
                mainNetworkBiases = mainNetworkLayer.Biases;
                targetNetworkBiases = targetNetworkLayer.Biases;
                for (int bi = 0; bi < layerBiasCount; bi++)
                    targetNetworkBiases[bi] = math.lerp(targetNetworkBiases[bi], mainNetworkBiases[bi], updateFactor);
            }
        }

        SoftUpdate();
    }

    private void OnApplicationQuit()
    {
        SaveNetwork();
    }
    [ContextMenu("Save")]
    public void SaveNetwork()
    {
        SaveNetwork(Path.Combine(Application.streamingAssetsPath, version + ".txt"));
    }
    public void SaveNetwork(string filePath)
    {
        string data = JsonUtility.ToJson(network);
        File.WriteAllText(filePath, data);
    }
    public void LoadNetwork()
    {
        if (clone)
        {
            if (clonedNetwork == null)
                return;
            string data = clonedNetwork.text;
            network = JsonUtility.FromJson<Network>(data);
        }
        else
        {
            if (version == "")
                return;
            string filePath = Path.Combine(Application.streamingAssetsPath, version + ".txt");

            if (!File.Exists(filePath))
                return;

            string data = File.ReadAllText(filePath);
            network = JsonUtility.FromJson<Network>(data);
        }
    }
    private void OnValidate()
    {
        if (Application.isPlaying && replayBuffer != null)
        {
            if (replayBuffer.maxSize < bufferSize)
                replayBuffer.buffer.RemoveRange(0, bufferSize - replayBuffer.maxSize);
            replayBuffer.maxSize = bufferSize;
        }
    }
    [Button("Version++")]
    public void ChangeVersion()
    {
        string lcid = version[version.Length - 1].ToString();
        int iid = 0;
        if (int.TryParse(lcid, out iid))
        {
            version = version.Remove(version.Length - 1);
            iid++;
            version += iid;
        }
        else
        {
            version += "_0";
        }

    }

    private Network DuplicateNetwork(Network network)
    {
        Network newNet = new Network();
        newNet.Layers = new List<Layer>();
        newNet.LearningRate = network.LearningRate;
        newNet.hiddenAType = network.hiddenAType;
        newNet.outputAType = network.outputAType;
        newNet.layersSize = new int[network.layersSize.Length];
        network.layersSize.CopyTo(newNet.layersSize, 0);
        newNet.Init();
        //targetNetwork.Init();

        return newNet;
    }

    public bool isNan => Application.isPlaying && float.IsNaN(network.Layers[0].Weights[0]);
    [ReadOnly,ShowIf("isNan"), Label("NAAAAAAAAAAAAAAAAAAN!!!!!!!!!!!!!!")]
    public bool Nannn;

    private void OnDestroy()
    {
        foreach (var layer in network.Layers)
        {
            
            layer.na_inputs.Dispose();
            layer.na_Weights.Dispose();
            layer.na_Biases.Dispose();
            layer.na__Outputs.Dispose();
            layer.na_activatedValues.Dispose();
        }
        foreach (var layer in targetNetwork.Layers)
        {
            layer.na_inputs.Dispose();
            layer.na_Weights.Dispose();
            layer.na_Biases.Dispose();
            layer.na__Outputs.Dispose();
            layer.na_activatedValues.Dispose();
            //
        }
    }
}
[Serializable]
public struct Transition
{
    public float[] prev_state;
    public int action;
    public float[] state_;
    public float reward;
    public bool isDone;
    //public Transition(float[] state, int action, float[] state_, float reward, bool isDone)
    //{
    //    Set(state, action, state_, reward, isDone);
    //    this.isDone = isDone;
    //}
    
    public void Set(float[] state, int action, float[] state_, float reward, bool isDone = false)
    {
        if (this.prev_state == null)
            this.prev_state = new float[state.Length];
        if (this.state_ == null)
            this.state_ = new float[state_.Length];
        state.CopyTo(this.prev_state, 0);
        this.action = action;
        state_.CopyTo(this.state_, 0);
        this.reward = reward;
        this.isDone = isDone;
    }
    
}
