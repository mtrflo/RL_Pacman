using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LandControl : MonoBehaviour {

    private Sequence landSequence;
    public float speed;
	// Use this for initialization
	void Start () {
        // land continue moving
        landSequence = DOTween.Sequence();
        float size = 0.48f;
        landSequence.Append(transform.DOMoveX(transform.position.x - size, size/speed).SetEase(Ease.Linear))
            .Append(transform.DOMoveX(transform.position.x, 0f).SetEase(Ease.Linear))
            .SetLoops(-1);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GameOver()
    {
        landSequence.Kill();
    }
}
