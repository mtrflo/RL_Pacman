using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
namespace MonoRL
{

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

        public void Learn(Transition transition)
        {
            replayBuffer.Add(transition);
            replayBuffer.maxSize = bufferSize;
            if (batchSize <= replayBuffer.maxSize)
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

        public void ReplaceTarget()
        {
            for (int i = 0; i < network.Layers.Count; i++)
            {
                for (int j = 0; j < network.Layers[i].Weights.Count; j++)
                    for (int k = 0; k < network.Layers[i].Weights[j].weigths.Count; k++)
                        targetNetwork.Layers[i].Weights[j].weigths[k] = network.Layers[i].Weights[j].weigths[k];

                network.Layers[i].Biases.CopyTo(targetNetwork.Layers[i].Biases, 0);
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
    }
}
