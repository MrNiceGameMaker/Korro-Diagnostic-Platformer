using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 3;
    void Update()
    {
        transform.Rotate(0,rotateSpeed*Time.deltaTime,0);
    }
}
