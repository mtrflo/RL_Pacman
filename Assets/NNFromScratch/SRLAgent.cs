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
        public Network network,targetNetwork;

        private void Awake()
        {
            if (me != null)
            {
                Destroy(gameObject);
                return;
            }
            me = this;
            network = new Network(lr);
            targetNetwork = new Network(lr);
            DontDestroyOnLoad(this);
        }

        public int SelectAction(double[] observation)
        {
            int action = 0;
            float e = Random.Range(0, 1f);

            if (e > epsilon)
            {
                double[] actionValues = network.Forward(observation);
                print("actionValues : " + actionValues.ToCommaSeparatedString());
                //if (actionValues[0] == double.NaN)
                //    Debug.Log("NAAAAAAAAAAAAAAAAAAAAAAAAAN");
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
                expectedValues[i] = predictedValues[i];// predictedValues[i] * - (QTarget - QEval);
            expectedValues[transition.action] = QTarget;
            
            network.Backward(transition.state, expectedValues);
        }

        public void ReplaceTarget()
        {
            for (int i = 0; i < network.Layers.Count; i++)
            {
                targetNetwork.Layers[i].Weights = network.Layers[i].Weights;
                targetNetwork.Layers[i].Biases = network.Layers[i].Biases;
            }
        }
        [ContextMenu("Save")]
        public void SaveNetwork()
        {
            string data = JsonUtility.ToJson(network);
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath,"network.net"),data);
        }
        public void LoadNetwork()
        {

        }
    }
}
