using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class MultitouchTest : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabPool;

    private Dictionary<Touch, GameObject> _touchSpheres = new();
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Touch.activeTouches.Count < 1)
            return;

        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                SpawnTouch(touch);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                UpdatePosition(touch);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                RemoveTouch(touch);
            }
        }

    }
    
    private void SpawnTouch(Touch touch)
    {
        var idx = Random.Range(0, _prefabPool.Length);

        _touchSpheres[touch] = Instantiate(_prefabPool[idx], ScreenToWorldPosition(touch.screenPosition), _prefabPool[idx].transform.rotation);
    }

    private void UpdatePosition(Touch touch)
    {
        _touchSpheres[touch].transform.position = ScreenToWorldPosition(touch.screenPosition);
    }

    private void RemoveTouch(Touch touch)
    {
        if (!_touchSpheres.TryGetValue(touch, out var sphere))
            return;
        
        Destroy(sphere);
        _touchSpheres.Remove(touch);
    }
    
    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        float distanceFromCamera = Mathf.Abs(
            0f - _camera.transform.position.z
        );

        Vector3 worldPosition = _camera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                distanceFromCamera
            )
        );

        worldPosition.z = 0;

        return worldPosition;
    }
}