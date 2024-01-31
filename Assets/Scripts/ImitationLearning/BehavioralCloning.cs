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
    public bool supervised = true;
    public bool rt = false;
    private void Start()
    {
        transitions = TransitionRecorder.me.LoadTransitions(demonstrator);
        print("supervised : " + supervised);
        StartCloning();
    }
    public void StartCloning()
    {
        if(rt)
            StartCoroutine(IERTCloning());
        else
            StartCoroutine(IECloning());
    }
    public float delay;
    IEnumerator IECloning()
    {
        float tlr = dqn.network.LearningRate;
        dqn.network.LearningRate = lr;
        int count = transitions.transitions.Count;
        for (int j = 0; j < repeat; j++)
        {
            for (int i = 0; i < count; i++)
            {
                progress = (j *1f/ repeat) + (0.1f *( i * 1f / count));
                if(supervised)
                    dqn.LearnSupervised(transitions.transitions[i]);
                else
                    dqn.Learn(transitions.transitions[i]);
                if (i % yieldEvery == 0)
                    yield return null;
            }
            //progress = j * 1f / repeat;
        }
        dqn.network.LearningRate = tlr;
        dqn.SaveNetwork(filePath);
        print("cloned");
        yield return null;

    }
    IEnumerator IERTCloning()
    {
        WaitForSeconds wfs = new WaitForSeconds(delay);
        int step = 0;
        while (true)
        {
            dqn.Learn(transitions.transitions[Random.Range(0, transitions.transitions.Count)]);
            if (step % yieldEvery == 0)
                yield return wfs;
            step++;
        }
    }
    public string filePath => Path.Combine(Application.dataPath, folderPath, "clones", fileName + ".txt");
}
