using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float moveDistance = 3f;

    private Vector3 startPosition;
    private int direction = 1; // Default direction

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);

        if (Mathf.Abs(transform.position.x - startPosition.x) >= moveDistance)
        {
            direction *= -1;
        }
    }

    public void SetDirection(int dir)
    {
        direction = dir; // Allows direction to be set from PlatformSpawner
    }
}