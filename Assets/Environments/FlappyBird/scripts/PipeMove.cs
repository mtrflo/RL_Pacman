using UnityEngine;
using System.Collections;

public class PipeMove : MonoBehaviour {
	public float moveSpeed;

	public Transform topPoint_l, topPoint_r, bottomPoint_l, bottomPoint_r;

    // Use this for initialization
    void Start () {
		Rigidbody2D body = transform.GetComponent<Rigidbody2D>();
		body.velocity = new Vector2(moveSpeed, 0);
		StartCoroutine(IEUpdate());

    }

	// Update is called once per frame
	IEnumerator IEUpdate () {
		WaitForSeconds wfs = new WaitForSeconds(0.5f);
		
		while (transform.position.x >= -0.4f) 
			yield return wfs;
		Destroy(gameObject);
	}

	public void GameOver()
	{
		Rigidbody2D body = transform.GetComponent<Rigidbody2D>();
		body.velocity = new Vector2(0, 0);
	}
}
