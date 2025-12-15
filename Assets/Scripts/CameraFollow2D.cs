using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;   
    public float smooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
