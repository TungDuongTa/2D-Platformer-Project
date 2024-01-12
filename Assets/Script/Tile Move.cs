using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMove : MonoBehaviour
{
    public float moveSpeed = 1.0f; // Adjust the speed as needed

    private void Start()
    {
        // Start the coroutine to move the tile map every second
        StartCoroutine(MoveTileMap());
    }

    private IEnumerator MoveTileMap()
    {
        while (true)
        {
            // Move the tile map in the desired direction (e.g., to the right)
            transform.position += Vector3.right * -moveSpeed * Time.deltaTime;

            // Wait for one second
            yield return new WaitForSeconds(1.0f);
        }
    }
}
