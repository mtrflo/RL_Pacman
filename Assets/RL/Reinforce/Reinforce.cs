using NaughtyAttributes;
using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

class CategoricalDistribution
{
    private float[] probabilities;
    private float[] cumulativeProbabilities;
    private int numCategories;

    public CategoricalDistribution(float[] probabilities)
    {
        this.probabilities = probabilities;
        this.numCategories = probabilities.Length;
        this.cumulativeProbabilities = new float[numCategories];

        // Compute cumulative probabilities
        float sum = 0;
        for (int i = 0; i < numCategories; i++)
        {
            sum += probabilities[i];
            cumulativeProbabilities[i] = sum;
        }
    }

    public int Sample()
    {
        float rand = UnityEngine.Random.Range(0, 0.99f);
        for (int i = 0; i < numCategories; i++)
        {
            if (rand < cumulativeProbabilities[i])
            {
                return i;
            }
        }
        return numCategories - 1; // edge case
    }

    public float LogProb(int category)
    {
        if (category < 0 || category >= probabilities.Length)
        {
            throw new ArgumentException("Invalid category index.");
        }
        return MathF.Log(probabilities[category]);
    }
}

public class Reinforce : MonoBehaviour
{
    public static Reinforce me;
    public Network network;

    [Range(0, 1)]
    public float discountFactor = 0.99f;
    public float learningRate = 0.0001f;
    public int trajectoryLength = 100;

    public string version = "test";

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
    }

    public int ChooseAction(float[] state) 
    {
        float[] actionValues = network.Forward(state);
        int action = actionValues.ToList().IndexOf(actionValues.Max());
        return action;
    }

    public int SampleAction(float[] state)
    {
        float[] actionProbs = network.Forward(state);
        CategoricalDistribution catDist = new CategoricalDistribution(actionProbs);
        return catDist.Sample();
    }

   



    public void Learn(Transition[] transitions)
    {
        float G = 0;

        for (int k = transitions.Length - 1; k >= 0; k--)
        {
            Transition transition = transitions[k];

            G += MathF.Pow(discountFactor, k) * transition.reward;

            float[] actionProbs = network.Forward(transition.prev_state);
            CategoricalDistribution categoricalDist = new CategoricalDistribution(actionProbs);
            float logProb = categoricalDist.LogProb(transition.action);
            float loss = -logProb * G;

            network.Backward(transition.action, loss);
        }
    }

    #region SaveLoad
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
    #endregion
}