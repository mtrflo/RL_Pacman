using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using UnityEngine;

public class BehavioralCloning : MonoBehaviour
{
    public TextAsset demonstrator;
    public string fileName, folderPath;
    public float lr;
    public float progress;
    public int repeat = 1;
    public int yieldEvery = 2;
    private Transitions transitions;
    public DQNAgent dqn;
    private void Start()
    {
        transitions = TransitionRecorder.me.LoadTransitions(demonstrator);
        StartCloning();
    }
    public void StartCloning()
    {
        StartCoroutine(IECloning());
    }
    IEnumerator IECloning()
    {
        float tlr = dqn.network.LearningRate;
        dqn.network.LearningRate = lr;
        int count = transitions.transitions.Count;
        for (int j = 0; j < repeat; j++)
        {
            for (int i = 0; i < count; i++)
            {
                progress = i * 1f / count;
                //dqn.Learn(transitions.transitions[i]);
                dqn.LearnSupervised(transitions.transitions[i]);
                if (i % yieldEvery == 0)
                    yield return null;
            }
            //progress = j * 1f / repeat;
        }
        dqn.network.LearningRate = tlr;
        print("dqn.network.LearningRate : " + dqn.network.LearningRate);
        dqn.SaveNetwork(filePath);
        print("cloned");
        yield return null;

    }

    public string filePath => Path.Combine(Application.dataPath, folderPath, "clones", fileName + ".txt");
}
