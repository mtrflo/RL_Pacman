using MathNet.Numerics.Distributions;
using NaughtyAttributes;
using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class Reinforce_Chain : MonoBehaviour
{
    public static Reinforce_Chain me;

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
    public int SelectAction(float[] observation, float _epsilon)
    {
        int action = 0;
        float e = UnityEngine.Random.Range(0, 1f);
        if (e > _epsilon)
        {
            action = ChooseAction(observation);
        }
        else
            //action = SampleAction(observation);
            action = UnityEngine.Random.Range(0, network.Layers.Last().NodeSize);

        return action;
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
        for (int t = trajectoryLength - 1; t > 0; t--)
            G +=   (discountFactor * (trajectoryLength - t) /t )  * transitions[t].reward ;

        G = Mathf.Exp(1) * G / (trajectoryLength );

        Transition transition = transitions.Last();


        float[] predictedValues = network.Forward(transition.prev_state);
        float QEval = predictedValues[transition.action];
        float QNext = network.Forward(transition.state_).Max();
        float QTarget = G + discountFactor * QNext;
        
        float loss = QEval - QTarget;
        //float loss = QTarget - QEval;
        
        network.Backward(transition.action, loss);

        //Debug.Log("transition.action : " + transition.action);
        Debug.Log("loss : " + loss);

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