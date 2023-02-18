using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPacManController : MonoBehaviour
{
    [SerializeField]
    private float moveUnitLength = 0.5f, duration = 0.5f;
    [SerializeField]
    private PacMapGenerator packMapGenerator;

    public Action OnMoved;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Horizontal") > 0) MoveToSide(1, 0);
        if (Input.GetAxis("Horizontal") < 0) MoveToSide(-1, 0);
        if (Input.GetAxis("Vertical") > 0) MoveToSide(0, 1);
        if (Input.GetAxis("Vertical") < 0) MoveToSide(0, -1);
    }
    private bool isMoving = false;
    public void MoveToSide(float xdir, float ydir)
    {
        Vector3 moveTo = new Vector3(transform.localPosition.x + xdir, transform.localPosition.y + ydir, transform.localPosition.z ) * moveUnitLength;
        if (isMoving)
            return;

        if (packMapGenerator.IsCooHasWall(moveTo))
        {
            
            reward = -100f;
            moveTo = transform.localPosition;
        }
        else
            reward = 1f;
        isMoving = true;

        transform.DOLocalMove(moveTo, duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            isMoving = false;
            packMapGenerator.RefreshMapData();
            
            OnMoved.Invoke();
        });

        Animate(new Vector2(xdir,ydir));
    }
    public float reward = 0;
    void Animate(Vector2 dir)
    {
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("pacdot"))
        {
            reward = 1;
        }
    }
}
