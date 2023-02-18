using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Rendering;

public class PacMapGenerator : MonoBehaviour//Env Model
{
    public Vector2 mapSize;
    public Texture2D sampleMapImage;
    public List<List<int>> cur_map = new List<List<int>>(), start_static_map = new List<List<int>>();

    [Header("Packdot")]
    public GameObject packDot;
    public Transform pdParent;

    private Vector2 startPackmanPosition = new Vector2(1,1);
    private void Awake()
    {
        GeneratePackdotsFromImage();
    }
    public void GeneratePackdotsFromImage()
    {

        int width = sampleMapImage.width;
        int height = sampleMapImage.height;
        Color32[] pixs = sampleMapImage.GetPixels32();
        CellGroup dot_cg = new CellGroup() { cellType = CellType.pacdot, obj_transforms = new List<Transform>() };
        CellGroups.Add(dot_cg);
        for (int i = 0; i < width; i++)
        {
            cur_map.Add(new List<int>());
            start_static_map.Add(new List<int>());
            for (int j = 0; j < height; j++)
            {
                bool isAlpha = pixs[i * width + j].a == 0;
                cur_map[i].Add(isAlpha ? 3 : 2);
                start_static_map[i].Add(isAlpha ? 3 : 2);
                
                if (isAlpha && !(startPackmanPosition.x == i && startPackmanPosition.y == j))
                {
                    Transform pt = Instantiate(packDot, pdParent).transform;
                    pt.localPosition = new Vector3(j , i , 0);
                    dot_cg.obj_transforms.Add(pt);
                }
            }
        }
        RefreshMapData();
    }

    void PrintMap()
    {
        for (int i = 0; i < cur_map.Count; i++)
        {
            string p = "";
            for (int j = 0; j < cur_map[0].Count; j++)
            {
                p += cur_map[i][j];
            }
            print(p);
        }
    }

    #region MapData
    public List<CellGroup> CellGroups;
    [ContextMenu("RefreshMapData")]
    public void RefreshMapData()
    {
        for (int i = 0; i < start_static_map.Count; i++)
        {
            for (int j = 0; j < start_static_map[i].Count; j++)
                cur_map[i][j] = start_static_map[i][j];
        }
        
        for (int i = 0; i < CellGroups.Count; i++)
        {
            CellGroups[i].obj_transforms.RemoveAll(x => x == null);
            foreach (Transform t in CellGroups[i].obj_transforms)
            {
                cur_map[(int)t.localPosition.y][(int)t.localPosition.x] = (int)CellGroups[i].cellType;
            } 
        }
        //PrintMap();
    }
    
    public bool IsCooHasWall(Vector3 pos) => cur_map[(int)pos.y][(int)pos.x] == 2;


    #endregion
}
[Serializable]
public class CellGroup
{
    public CellType cellType;
    public List<Transform> obj_transforms;
}

[Serializable]
public enum CellType
{
    pacman=1,
    wall=2,
    ground=3,
    pacdot=4
}