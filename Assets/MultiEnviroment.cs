using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class MultiEnviroment : MonoBehaviour
{
    public static MultiEnviroment me;
    #region Serialized vars
    public int count;
    public GameObject env;
    public Transform ins_parent;
    public float space = 5;
    public bool3 boo;
    #endregion


    #region NonSerialized vars

    #endregion
    private void Awake()
    {
        if (me != null)
        {
            Destroy(gameObject);
            return;
        }
        me = this;

        if(ins_parent == null)
        {
            ins_parent = transform;
        }
    }
    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Vector3.one * ((i + 1) * space);
            offset.x *= boo.x == false ? 0 : 1;
            offset.y *= boo.y == false ? 0 : 1;
            offset.z *= boo.z == false ? 0 : 1;
            Instantiate(env, transform.position + offset, Quaternion.identity, ins_parent);
            //env_t.transform.Translate(, 0,0, Space.World);
        }
    }
}
