using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionRecorder : MonoBehaviour
{
    public static TransitionRecorder me;
    public string fileName;
    public int stepCount = 10000;
    public int addedTrCount = 0;
    string filePath;
    Transitions TransitionsData;
    
    private void Awake()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "records", fileName + ".txt");
        print("filePath : " + filePath);
        me = this;
        TransitionsData = new Transitions();
    }

    public void AddTransition(Transition transition)
    {
        if (addedTrCount > stepCount)
        {
            return;
        }
        else if (addedTrCount == stepCount)
            Record();
        else
        {
            TransitionsData.transitions.Add(transition);
            addedTrCount++;
        }
        
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