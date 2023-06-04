using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAgent
{
    public Transition _Transition
    {
        get
        {
            return _Transition;
        }
        set
        {
            _Transition = value;
        }
    }
    
    public void AddObservation();

    public void ChooseAction();

}
