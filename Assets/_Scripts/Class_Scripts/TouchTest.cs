using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchTest : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Iniciando Script");
    }
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        
        if (Touch.activeTouches.Count == 0)
        {
            return;
        }


        Touch touch = Touch.activeTouches[0];

        if (touch.phase == TouchPhase.Began)
        {
            ShowTouch(touch.screenPosition);
        }
    }

    private void ShowTouch(Vector2 screenPosition)
    {
        Debug.Log($"Touch detectado en pantalla: {screenPosition}");
    }
}