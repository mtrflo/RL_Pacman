﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PipeSpawner : MonoBehaviour {

	public float spawnTime = 5f;		// The amount of time between each spawn.
	public float spawnDelay = 3f;		// The amount of time before spawning starts.
	public PipeMove pipe;
	public float[] heights;

	//public TimeController timeController;

	private float startSpawnTime = 0;
    WaitForSecondsRealtime wfsr;
	private List<PipeMove> pipeMoves;

	public PipeMove lastPipe;
    private void Awake()
    {
        //timeController = TimeController.me;
        startSpawnTime = spawnTime;
		//timeController.ChangeVarsByTimeScale += ChangeVars;
	}
    void Start ()
	{
        // Start calling the Spawn function repeatedly after a delay .		

        pipeMoves = new List<PipeMove>();

        StartCoroutine(StartSpawning());

    }

    public IEnumerator StartSpawning()
    {
		yield return new WaitForSecondsRealtime(spawnDelay);
        //WaitForSecondsRealtime wfsr = new WaitForSecondsRealtime(spawnTime);
        wfsr = new WaitForSecondsRealtime(spawnTime);
        while (true)
		{
			Spawn();
			yield return wfsr;
        }
    }
	
	
	void Spawn ()
	{
		int heightIndex = Random.Range(0, heights.Length);
		Vector2 pos = new Vector2(transform.position.x, heights[heightIndex] + transform.position.y);
		if (pipe)
		{
            PipeMove pm = Instantiate(pipe, pos, transform.rotation,transform);
			pipeMoves.Add(pm);
			if (pipeMoves.Count == 1)
			{
				lastPipe = pipeMoves[0];
            }
		}
	}

	public void PipePassed()
	{
		pipeMoves.RemoveAt(0);
		lastPipe = pipeMoves[0];

    }

	public void GameOver()
	{
		CancelInvoke("Spawn");
	}

	void ChangeVars(float ts)
	{
        spawnTime = startSpawnTime / ts;
        wfsr = new WaitForSecondsRealtime(spawnTime);
    }
    private void OnDestroy()
    {
		//timeController.ChangeVarsByTimeScale -= ChangeVars;

    }
}
