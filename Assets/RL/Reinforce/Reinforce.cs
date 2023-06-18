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

    [Range(0, 1)]
    public float discountFactor = 0.99f;
    public int trajectoryLength = 100;

    public string version = "test";

    public Network network;
    
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
    private void Start()
    {
        
    }
    public int ChooseAction(float[] state) 
    {
        float[] actionValues = network.Forward(state);
        int action = actionValues.ToList().IndexOf(actionValues.Max());
        print("ChooseAction : " + action);
        return action;
    }

    public int SampleAction(float[] state)
    {
        float[] actionProbs = network.Forward(state);
        float[] sm_actionProbs = GetActionProbs(actionProbs);
        CategoricalDistribution catDist = new CategoricalDistribution(sm_actionProbs);
        return catDist.Sample();
    }
    public float[] GetActionProbs(float[] inputs)
    {
        float expSum = 0;
        float[] probs = new float[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
            expSum += Mathf.Exp(inputs[i]);
        for (int i = 0; i < inputs.Length; i++)
            probs[i] = Mathf.Exp(inputs[i]) / expSum;

        return probs;
    }




    public void Learn(Transition[] transitions)
    {
        float G = 0;

        for (int k = transitions.Length - 1; k >= 0; k--)
        {
            Transition transition = transitions[k];

            G += MathF.Pow(discountFactor, k) * transition.reward;

            float[] actionProbs = network.Forward(transition.prev_state);
            float[] sm_actionProbs = GetActionProbs(actionProbs);
            CategoricalDistribution catDist = new CategoricalDistribution(sm_actionProbs);
            
            float logProb = catDist.LogProb(transition.action);
            float loss = logProb * G;

            network.BackwardGPU(this,transition.action, loss);

            //Debug.Log("transition.action : " + transition.action);
            //Debug.Log("loss : " + loss);
        }
    }


    public bool isNan => Application.isPlaying && float.IsNaN(network.Layers[0].Weights[0]);
    [ReadOnly, ShowIf("isNan"), Label("NAAAAAAAAAAAAAAAAAAN!!!!!!!!!!!!!!")]
    public bool Nannn;

    #region SaveLoad Network
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