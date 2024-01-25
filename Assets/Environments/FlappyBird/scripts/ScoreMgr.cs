using UnityEngine;
using System.Collections;
using TMPro;

public class ScoreMgr : MonoBehaviour {

	public GameObject[] scorePrefabs;
	public float digitOffset;
	public TextMeshPro textMeshProUGUI;
    public int currentScore = 0;

    void Start()
    {
        currentScore = 0;
        SetScore(currentScore);
    }

    public void AddScore()
    {
        currentScore ++;
        SetScore(currentScore);
    }

    public void SetScore(int score)
	{
        textMeshProUGUI.text = score.ToString();
	}
}
