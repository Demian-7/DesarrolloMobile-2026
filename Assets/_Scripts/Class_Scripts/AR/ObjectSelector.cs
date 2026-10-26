using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    [Header("Drop here the selectable objects")]
    [SerializeField] private GameObject[] _selectableObjects;

    public void SelectObject(int index)
    {
        for (int i = 0; i < _selectableObjects.Length; i++)
        {
            _selectableObjects[i].SetActive(i == index);
        }
    }
}
