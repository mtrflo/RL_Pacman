using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionRecorder : MonoBehaviour
{
    public static TransitionRecorder me;
    public bool record = false, closeAfterAdded, realTimeRecord, priceAsTrigger;
    public string fileName, folderPath;
    public int stepCount = 10000;
    public int addedTrCount = 0;
    string filePath;
    [HideInInspector] public Transitions TransitionsData;
    public float minRewardPrice = 1, currentWaitersRewardSum = 0;
    public List<Transition> waiters;
    private void Awake()
    {
        filePath = Path.Combine(Application.dataPath, folderPath, "records", fileName + ".txt");
        me = this;
        waiters = new List<Transition>();
        TransitionsData = new Transitions();
    }
    bool isRecorded = false;
    private bool isGateOpened = false;
    public void AddTransition(Transition transition)
    {


        if (!record || isRecorded)
            return;



        if (isGateOpened)
        {
            TransitionsData.transitions.Add(transition);
            addedTrCount++;
        }
        else if (priceAsTrigger ? transition.reward <= minRewardPrice : currentWaitersRewardSum <= minRewardPrice && !isGateOpened)
        {
            currentWaitersRewardSum += transition.reward;
            waiters.Add(transition);
        }
        else
        {
            OpenGate();
            CloseGate();
        }

        if (transition.isDone)
            CloseGate();

        if (stepCount <= addedTrCount)
        {
            if (realTimeRecord)
            {
                TransitionsData.transitions.RemoveRange(0, waiters.Count);
            }
            else
            {
                Record();
                CloseGate();
            }
        }
    }

    void CloseGate()
    {
        waiters = new List<Transition>();
        currentWaitersRewardSum = 0;
        isGateOpened = false;
    }
    void OpenGate()
    {
        TransitionsData.transitions.AddRange(waiters);
        addedTrCount += waiters.Count;
        isGateOpened = true;
    }
    public void Record()
    {
        isRecorded = true;
        string data = JsonUtility.ToJson(TransitionsData);
        File.WriteAllText(filePath, data);
        print(stepCount + " steps recorded");
    }
    public Transitions LoadTransitions(TextAsset asset)
    {
        if (asset == null)
            return null;

        string data = asset.text;
        Transitions tr = JsonUtility.FromJson<Transitions>(data);
        return tr;
    }
}
[Serializable]
public class Transitions
{
    public List<Transition> transitions;
    public Transitions()
    {
        transitions = new List<Transition>();
    }
}