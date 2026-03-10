using UnityEngine;

public class Moving : MonoBehaviour
{
    
    public Vector3 moveDistance; // Distance to move
    public float speed = 2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        moveDistance = startPos + new Vector3(startPos.x + 5, startPos.y, startPos.z);
    }


    void Update()
    {
        // Calculate a value that oscillates between 0 and 1
        float movement = Mathf.PingPong(Time.time * speed, 1);
        transform.position = startPos + (moveDistance * movement);
    }
}
