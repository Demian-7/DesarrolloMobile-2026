using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class MultitouchTest : MonoBehaviour
{
    [SerializeField] private GameObject[] _prefabPool;

    private Dictionary<Touch, GameObject> _touchSpheres = new();

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
        _touchSpheres[touch] = Instantiate(_prefabPool[idx], touch.screenPosition, _prefabPool[idx].transform.rotation);
    }

    private void UpdatePosition(Touch touch)
    {
        _touchSpheres[touch].transform.position = touch.screenPosition;
    }

    private void RemoveTouch(Touch touch)
    {
        _touchSpheres.Remove(touch);
    }
}