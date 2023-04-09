using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiEnviroment : MonoBehaviour
{
    #region Serialized vars
    public int count;
    public GameObject env;
    public Transform ins_parent;
    public float space = 5;
    #endregion


    #region NonSerialized vars

    #endregion
    private void Awake()
    {
        if(ins_parent == null)
        {
            ins_parent = transform;
        }
    }
    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject env_t = Instantiate(env, ins_parent);
            env_t.transform.Translate((i+1) * space, 0,0, Space.World);
        }
    }
}
