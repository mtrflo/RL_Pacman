using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionRecorder : MonoBehaviour
{
    public static TransitionRecorder me;
    public bool record = false;
    public string fileName, folderPath;
    public int stepCount = 10000;
    public int addedTrCount = 0;
    string filePath;
    Transitions TransitionsData;
    public float minRewardPrice = 1, currentWaitersRewardSum = 0;
    public List<Transition> waiters;
    private void Awake()
    {
        filePath = Path.Combine(Application.dataPath, folderPath, "records", fileName + ".txt");
        print("filePath : " + filePath);
        me = this;
        waiters = new List<Transition>();
        TransitionsData = new Transitions();
    }
    bool isRecorded = false;
    public void AddTransition(Transition transition)
    {
        

        if(!record)
            return;

        if (isRecorded)
            return;

        

        if (addedTrCount == stepCount)
        {
            isRecorded = true;
            Record();
        }
        TransitionsData.transitions.Add(transition);
        addedTrCount++;
    }

    public void Record()
    {
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