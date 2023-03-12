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
        public string version = "test";
        public Network network, targetNetwork;
        private void Awake()
        {
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
            double[] predictedValues = network.Forward(transition.state);
            double QEval = predictedValues[transition.action];
            double QNext = targetNetwork.Forward(transition.state_).Max();
            double QTarget = transition.reward + gamma * QNext;
            if (transition.isDone)
                QTarget = transition.reward;

            double[] expectedValues = new double[predictedValues.Length];
            for (int i = 0; i < predictedValues.Length; i++)
                expectedValues[i] = predictedValues[i];// * - (QTarget - QEval);
            expectedValues[transition.action] = QTarget;

            network.Backward(transition.state, expectedValues);
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
            if (version == "" )
                return;
            string filePath = Path.Combine(Application.streamingAssetsPath, version + ".txt");

            if (!File.Exists(filePath))
                return;

            string data = File.ReadAllText(filePath);
            network = JsonUtility.FromJson<Network>(data);
        }
    }
}
