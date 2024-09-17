using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownController : MonoBehaviour
{
    [SerializeField] private Dropdown hourDropdown;
    [SerializeField] private Dropdown minuteDropdown;

    public event Action OnDropdownChanged;

    private void Start()
    {
        FillDropdowns();
        
        hourDropdown.onValueChanged.AddListener(_ => OnDropdownChanged?.Invoke());
        minuteDropdown.onValueChanged.AddListener(_ => OnDropdownChanged?.Invoke());
    }

    private void OnDestroy()
    {
        hourDropdown.onValueChanged.RemoveListener(_ => OnDropdownChanged?.Invoke());
        minuteDropdown.onValueChanged.RemoveListener(_ => OnDropdownChanged?.Invoke());
    }

    // Заполнение списков всеми возможными значениями
    private void FillDropdowns()
    {
        hourDropdown.ClearOptions();
        minuteDropdown.ClearOptions();

        var hourOptions = new List<string>();
        for (var i = 0; i < 24; i++)
        {
            hourOptions.Add(i.ToString("00"));
        }

        var minuteOptions = new List<string>();
        for (var i = 0; i < 60; i++)
        {
            minuteOptions.Add(i.ToString("00"));
        }

        hourDropdown.AddOptions(hourOptions);
        minuteDropdown.AddOptions(minuteOptions);
    }

    public int GetSelectedHour()
    {
        return hourDropdown.value;
    }

    public int GetSelectedMinute()
    {
        return minuteDropdown.value;
    }

    public void UpdateDropdowns(int hour, int minute)
    {
        hourDropdown.value = hour;
        minuteDropdown.value = minute;
    }
}
