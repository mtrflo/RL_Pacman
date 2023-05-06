using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class GameMain : MonoBehaviour {

    public GameObject bird;
    public GameObject readyPic;
    public GameObject tipPic;
    public GameObject scoreMgr;

    private bool gameStarted = false;
    public Action OnGameStarted;
	// Use this for initialization
	void Start () {
        OnGameStarted = () => { };
        StartGame();
    }
	
	// Update is called once per frame
	//void Update () {
 //       if (!gameStarted && Input.GetButtonDown("Fire1"))
 //       {
 //           gameStarted = true;
 //           StartGame();
 //       }
 //   }

    private void StartGame()
    {
        BirdControl control = bird.GetComponent<BirdControl>();
        control.inGame = true;
        control.JumpUp();

        readyPic.GetComponent<SpriteRenderer>().material.DOFade(0f, 0.2f);
        tipPic.GetComponent<SpriteRenderer>().material.DOFade(0f, 0.2f);

        scoreMgr.GetComponent<ScoreMgr>().SetScore(0);
        OnGameStarted.Invoke();
    }
}
