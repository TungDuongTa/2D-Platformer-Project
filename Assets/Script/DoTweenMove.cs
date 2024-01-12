using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoTweenMove : MonoBehaviour
{
    public Transform EndPosition;
    // Start is called before the first frame update
    void Start()
    {
        Vector2 target = EndPosition.position;
        transform.DOMove(target, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
