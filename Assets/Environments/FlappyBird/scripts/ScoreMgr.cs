using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreMgr : MonoBehaviour {

	public GameObject[] scorePrefabs;
	public float digitOffset;
	public TextMeshPro textMeshProUGUI;
    private int nowScore = 0;

    void Start()
    {
        nowScore = 0;
        SetScore(nowScore);
    }

    public void AddScore()
    {
        nowScore ++;
        SetScore(nowScore);
    }

    public void SetScore(int score)
	{
        textMeshProUGUI.text = score.ToString();
	}
}
