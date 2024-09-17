using UnityEngine;

public class ClockHand : MonoBehaviour
{
    [SerializeField] private RectTransform handTransform;
    private float _currentRotation;

    public void UpdateHand(float angle)
    {
        _currentRotation = angle;
        handTransform.localRotation = Quaternion.Euler(0, 0, -_currentRotation);
    }
}
