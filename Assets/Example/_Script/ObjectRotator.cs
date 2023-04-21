using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up, 100 * Time.deltaTime);
    }
}
