using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;
    public float smoothing;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPosition = Vector3.Lerp(transform.position, target.position + offset, smoothing * Time.deltaTime);
            transform.position = newPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}