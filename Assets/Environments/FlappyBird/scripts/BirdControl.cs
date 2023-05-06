﻿using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class BirdControl : MonoBehaviour {

	public int rotateRate = 10;
	public float upSpeed = 10;
    public ScoreMgr scoreMgr;

    public AudioClip jumpUp;
    public AudioClip hit;
    public AudioClip score;
    public PipeSpawner pipeSpawner;

    public bool inGame = false;

	public bool dead = false;

	private bool landed = false;

    private Sequence birdSequence;

	public Action OnDie;
    public Action OnPipePassed;
	private Animator animator;
    private void Awake()
    {
        OnDie = () => { };
        
        OnPipePassed = () => { };
    }
    // Use this for initialization
    void Start () {

        animator = GetComponent<Animator>();
        float birdOffset = 0.05f;
        float birdTime = 0.3f;
        float birdStartY = transform.position.y;

        birdSequence = DOTween.Sequence();

        birdSequence.Append(transform.DOMoveY(birdStartY + birdOffset, birdTime).SetEase(Ease.Linear))
            .Append(transform.DOMoveY(birdStartY - 2 * birdOffset, 2 * birdTime).SetEase(Ease.Linear))
            .Append(transform.DOMoveY(birdStartY, birdTime).SetEase(Ease.Linear))
            .SetLoops(-1);
    }
	
	// Update is called once per frame
	void Update () {
        if (!inGame)
        {
            return;
        }
        birdSequence.Kill();

		if (!dead)
		{
			if (Input.GetButtonDown("Fire1"))
			{
                JumpUp();
			}
		}

		if (!landed)
		{
			float v = transform.GetComponent<Rigidbody2D>().velocity.y;
			
			float rotate = Mathf.Min(Mathf.Max(-90, v * rotateRate + 60), 30);
			
			transform.rotation = Quaternion.Euler(0f, 0f, rotate);
		}
		else
		{
			transform.GetComponent<Rigidbody2D>().rotation = -90;
		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.name == "land" || other.name == "pipe_up" || other.name == "pipe_down")
		{
            //if (other.name == "pipe_up" || other.name == "pipe_down")
            //{
            //    pipe = other.transform.parent.GetComponent<PipeMove>().nextPipe;
            //    //Destroy(other.transform.parent.gameObject);
            //}
            if (!dead)
            {
                animator.SetTrigger("die");
				GameOver();
				OnDie();
            }
			

			if (other.name == "land")
			{
				transform.GetComponent<Rigidbody2D>().gravityScale = 0;
				transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

				landed = true;
			}
		}

        if (other.name == "pass_trigger")
        {
            scoreMgr.AddScore();
            pipeSpawner.PipePassed();
            OnPipePassed();
            //AudioSource.PlayClipAtPoint(score, Vector3.zero);
        }


	}

    public void JumpUp()
    {
        transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, upSpeed);
        //AudioSource.PlayClipAtPoint(jumpUp, Vector3.zero);
    }
	
	public void GameOver()
	{
		dead = true;

        //FlappyBirdAgent.birdsCount--;
        //if (FlappyBirdAgent.birdsCount <= 0)
        //{
        //    FlappyBirdAgent.birdsCount = 0;
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //}
    }

    public void ResetComponent()
    {
        animator.ResetTrigger("die");
        dead = false;
        landed = false;
    }
}
