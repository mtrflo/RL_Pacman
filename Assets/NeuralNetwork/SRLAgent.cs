using MonoRL;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
public class SRLAgent : MonoBehaviour
{
    public static SRLAgent me;
    [Range(0, 1)]
    public float epsilon = 0.8f;//exploit - explore     0-1
    [Range(0, 1)]
    public float gamma = 0.8f;
    [Range(0, 1)]
    public float lr = 0.1f;
    [Header("ReplayBuffer")]
    public int bufferSize = 500;
    public int batchSize = 50;
    public int pCount = 1;
    public string version = "test";
    public Network network, targetNetwork;
    public ReplayBuffer<Transition> replayBuffer;
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
        targetNetwork.Init();
        DontDestroyOnLoad(this);
    }
    public int SelectAction(double[] observation)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > epsilon)
        {
            double[] actionValues = network.Forward(observation);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, network.Layers.Last().NodeSize);

        return action;
    }

    public int SelectAction(double[] observation,float _epsilon)
    {
        int action = 0;
        float e = Random.Range(0, 1f);

        if (e > _epsilon)
        {
            double[] actionValues = network.Forward(observation);
            action = actionValues.ToList().IndexOf(actionValues.Max());
        }
        else
            action = Random.Range(0, network.Layers.Last().NodeSize);

        return action;
    }

    public void Learn(Transition transition)
    {
        replayBuffer.Add(transition);

        if (replayBuffer.Size() >= batchSize)
        {
            for (int e = 0; e < pCount; e++)
            {

                Transition[] randomSamples = replayBuffer.GetRandomSamples(batchSize);

                double[][] batchInputs = randomSamples.Select(x => x.state).ToArray();
                double[][] batchExpectedOutputs = new double[batchInputs.Length][];

                for (int batchIndex = 0; batchIndex < batchSize; batchIndex++)
                {
                    Transition sampleTransition = randomSamples[batchIndex];
                    batchInputs[batchIndex] = sampleTransition.state;

                    double[] predictedValues = network.Forward(sampleTransition.state);
                    double QEval = predictedValues[sampleTransition.action];
                    double QNext = targetNetwork.Forward(sampleTransition.state_).Max();
                    double QTarget = sampleTransition.reward + gamma * QNext;
                    if (sampleTransition.isDone)
                        QTarget = sampleTransition.reward;

                    double[] expectedValues = new double[predictedValues.Length];
                    for (int i = 0; i < predictedValues.Length; i++)
                        expectedValues[i] = predictedValues[i];// * - (QTarget - QEval);
                    expectedValues[sampleTransition.action] = QTarget;

                    batchExpectedOutputs[batchIndex] = expectedValues;
                }

                network.Learn(batchInputs, batchExpectedOutputs);
            }
        }
    }

    public void ReplaceTarget()
    {
        int mainNetworkLayerCount = network.Layers.Count;
        int mainNetworkLayerWeightCount = 0;
        int mainNetworkLayersWeightsWeigthsCount = 0;
        for (int i = 0; i < mainNetworkLayerCount; i++)
        {
            mainNetworkLayerWeightCount = network.Layers[i].Weights.Count;
            Layer mainNetworkLayer = network.Layers[i],
                  targetNetworkLayer = targetNetwork.Layers[i];
            for (int j = 0; j < mainNetworkLayerWeightCount; j++)
            {
                Weights t_weights = targetNetworkLayer.Weights[j], 
                        m_weights = mainNetworkLayer.Weights[j];
                mainNetworkLayersWeightsWeigthsCount = mainNetworkLayer.Weights[j].weigths.Count;
                for (int k = 0; k < mainNetworkLayersWeightsWeigthsCount; k++)
                {
                    t_weights.weigths[k] = m_weights.weigths[k];
                }
                mainNetworkLayer.Biases.CopyTo(targetNetworkLayer.Biases, 0);
            }
        }
    }
    private void OnApplicationQuit()
    {
        SaveNetwork();
    }
    [ContextMenu("Save")]
    public void SaveNetwork()
    {
        string data = JsonUtility.ToJson(network);
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, version + ".txt"), data);
    }
    public void LoadNetwork()
    {
        if (version == "")
            return;
        string filePath = Path.Combine(Application.streamingAssetsPath, version + ".txt");

        if (!File.Exists(filePath))
            return;

        string data = File.ReadAllText(filePath);
        network = JsonUtility.FromJson<Network>(data);
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
    public bool isNan => Application.isPlaying && Double.IsNaN(network.Layers[0].Weights[0].weigths[0]);
    [ReadOnly,ShowIf("isNan"), Label("NAAAAAAAAAAAAAAAAAAN!!!!!!!!!!!!!!")]
    public bool Nannn;

}
[Serializable]
public class Transition
{
    public double[] state;
    public int action;
    public double[] state_;
    public double reward;
    public bool isDone;
    public Transition(double[] state, int action, double[] state_, double reward, bool isDone)
    {
        Set(state, action, state_, reward, isDone);
        this.isDone = isDone;
    }
    public Transition()
    {

    }
    public void Set(double[] state, int action, double[] state_, double reward, bool isDone = false)
    {
        if (this.state == null)
            this.state = new double[state.Length];
        if (this.state_ == null)
            this.state_ = new double[state_.Length];
        state.CopyTo(this.state, 0);
        this.action = action;
        state_.CopyTo(this.state_, 0);
        this.reward = reward;
        this.isDone = isDone;
    }
}