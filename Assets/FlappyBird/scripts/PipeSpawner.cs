using UnityEngine;
using System.Collections;

public class PipeSpawner : MonoBehaviour {

	public float spawnTime = 5f;		// The amount of time between each spawn.
	public float spawnDelay = 3f;		// The amount of time before spawning starts.
	public GameObject pipe;	
	public float[] heights;
	
	
	void Start ()
	{
		// Start calling the Spawn function repeatedly after a delay .		
		StartCoroutine(StartSpawning());

    }

    public IEnumerator StartSpawning()
    {
		yield return new WaitForSecondsRealtime(spawnDelay);
		WaitForSecondsRealtime wfsr = new WaitForSecondsRealtime(spawnTime);
		while (true)
		{
			yield return wfsr;
			Spawn();
        }
    }
	
	
	void Spawn ()
	{
		int heightIndex = Random.Range(0, heights.Length);
		Vector2 pos = new Vector2(transform.position.x, heights[heightIndex]);
		if(pipe)
			Instantiate(pipe, pos, transform.rotation);
	}

	public void GameOver()
	{
		CancelInvoke("Spawn");
	}
}
