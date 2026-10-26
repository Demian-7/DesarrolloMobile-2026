using UnityEngine;

public class UIRotator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(15f, 45f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
    }
}