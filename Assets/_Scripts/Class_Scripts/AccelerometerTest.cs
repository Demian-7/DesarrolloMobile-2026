using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AccelerometerTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider _energyBar;
    [Header("Setting values")]
    [SerializeField] private float _baselineAcceleration;
    [SerializeField] private float _smoothingStrength;
    [Header("Energy values")]
    [SerializeField] private float _decreaseRate;
    [SerializeField] private float _increaseRate;

    private float _shakeCooldown = 0.5f;
    private float _lastShakeTime = float.MinValue;
    private Vector3 _currentAccValue;
    private Vector3 _pastAccValue;
    private bool _canShake;

    private void Update()
    {
        if (Accelerometer.current == null)
        {
            Debug.Log("No accelerometer detected!");
            return;
        }

        

        Debug.Log("H");

        _currentAccValue = Accelerometer.current.acceleration.ReadValue();

        if (!IsShaking())
        {
            UpdateUI(false);
            return;
        }

        UpdateUI(true);

        _pastAccValue = _currentAccValue;
    }

    private bool IsShaking() => _currentAccValue.magnitude >= _baselineAcceleration ? true : false;

    private void UpdateUI(bool isShaking)
    {
        if (isShaking)
        {
            var smoothed = Mathf.Lerp(_pastAccValue.magnitude, _currentAccValue.magnitude, Time.deltaTime * _smoothingStrength);
            _energyBar.value += Time.deltaTime * _increaseRate;
        }
        else
        {
            _energyBar.value -= Time.deltaTime * _decreaseRate;
        }
    }
}


//Shaking incrases a bar.
//Not chaking decreases the bar.