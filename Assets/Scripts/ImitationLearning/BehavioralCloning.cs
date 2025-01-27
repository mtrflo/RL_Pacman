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
    public bool realTime = false;
    public bool removeTerm = false;
    private void Start()
    {
        transitions = TransitionRecorder.me.LoadTransitions(demonstrator);
        print("supervised : " + supervised);
        StartCloning();
    }
    public void StartCloning()
    {
        if(realTime)
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
        Transition transition;
        for (int j = 0; j < repeat; j++)
        {
            for (int i = 0; i < count; i++)
            {
                transition = transitions.transitions[i];
                
                if (removeTerm && transitions.transitions[i].isDone)
                    continue;
                
                progress = (j *1f/ repeat) + (0.1f *( i * 1f / count));
                if(supervised)
                    dqn.LearnSupervised(transition);
                else
                    dqn.Learn(transition);
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
    public TransitionRecorder recorder;
    public int minRealTimeTransitionCount = 100;
    IEnumerator IERTCloning()
    {
        WaitForSeconds wfs = new WaitForSeconds(delay);
        List<Transition> transitions = recorder.TransitionsData.transitions;
        int step = 0;
        int count = transitions.Count;
        while (true)
        {
            count = transitions.Count;
            if (count > minRealTimeTransitionCount)
            {
                if (supervised)
                    dqn.LearnSupervised(transitions[Random.Range(0, count)]);
                else
                    dqn.Learn(transitions[Random.Range(0, count)]);
            }
            yield return wfs;
            step++;
        }
    }
    public string filePath => Path.Combine(Application.dataPath, folderPath, "clones", fileName + ".txt");
}
