using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform platform;
    public Transform StartPoint;
    public Transform EndPoint;
    int direction = 1;
    public float speed = 5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 target = CurrentMovement();
        platform.position = Vector2.Lerp(platform.position, target, speed * Time.deltaTime);
        float Distance = (target - (Vector2)platform.position).magnitude;
        if (Distance <= 0.1f) {
            direction *= -1;
        }
    }
    private void OnDrawGizmos()
    {
        if (platform != null && StartPoint != null && EndPoint != null) {
            Gizmos.DrawLine(platform.transform.position, StartPoint.position);
            Gizmos.DrawLine(platform.transform.position, EndPoint.position);
        }
    }
    Vector2 CurrentMovement() {
        if (direction == 1)
        {
            return StartPoint.position;
        }
        else { 
            return EndPoint.position;
        }
    }
}
