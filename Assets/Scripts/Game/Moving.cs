using UnityEngine;

public class Moving : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    public float speed = 2f;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private void Start()
    {
        _startPos = transform.position;
        _endPos = _startPos + moveOffset;
    }

    private void Update()
    {
        float movement = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector3.Lerp(_startPos, _endPos, movement);
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_startPos, _endPos);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + moveOffset);
        }
    }
}
