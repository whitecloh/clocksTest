using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AlarmClockController : MonoBehaviour, IDragHandler, IEndDragHandler 
{
    [SerializeField] private ClockHand hourHand;
    [SerializeField] private ClockHand minuteHand; 
    [SerializeField] private DropdownController dropdownController;
    [SerializeField] private NotificationsHandler notificationHandler;

    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform addictPanel;

    private int _selectedHour = 0;
    private int _selectedMinute = 0;
    private bool _isDragging = false;

    private const string AlarmHourKey = "AlarmHour";
    private const string AlarmMinuteKey = "AlarmMinute";

    private void Start()
    {
        dropdownController.OnDropdownChanged += UpdateClockFromDropdown;
        LoadSavedAlarm();
        UpdateClockFromDropdown();
    }

    private void Update()
    {
        if (!_isDragging)
        {
            UpdateHands();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        var direction = eventData.position - (Vector2)transform.position;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        angle = (angle + 360) % 360;
        angle = 360 - angle;

        _isDragging = true;
        UpdateClockFromHand(angle);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        dropdownController.UpdateDropdowns(_selectedHour, _selectedMinute);
    }

    private void OnDestroy()
    {
        dropdownController.OnDropdownChanged -= UpdateClockFromDropdown;
    }

    public void UI_SetAlarm()
    {
        SetAlarm();
        SaveAlarm();
    }

    public void UI_OpenMainPanel()
    {
        mainPanel.gameObject.SetActive(true);
        addictPanel.gameObject.SetActive(false);
    }

    public void UI_HideMainPanel()
    {
        mainPanel.gameObject.SetActive(false);
        addictPanel.gameObject.SetActive(true);
    }

    private void UpdateClockFromDropdown()
    {
        _selectedHour = dropdownController.GetSelectedHour();
        _selectedMinute = dropdownController.GetSelectedMinute();
        UpdateHands();
    }

    private void UpdateHands()
    {
        var hourAngle = (360f / 12f) * (_selectedHour % 12);
        var minuteAngle = (360f / 60f) * _selectedMinute;

        hourHand.UpdateHand(hourAngle);
        minuteHand.UpdateHand(minuteAngle);
    }

    private void UpdateClockFromHand(float angle)
    {
        var newMinutes = Mathf.FloorToInt(angle / 6) % 60;
        if (newMinutes < 0) newMinutes += 60;

        var deltaMinutes = newMinutes - _selectedMinute;

        if (Mathf.Abs(deltaMinutes) > 30)
        {
            if (deltaMinutes < 0)
                _selectedHour = (_selectedHour + 1) % 24;
            else
                _selectedHour = (_selectedHour - 1 + 24) % 24;
        }

        _selectedMinute = newMinutes;
        UpdateHands();
    }

    private void SetAlarm()
    {
        var alarmTime = CalculateAlarmTime(_selectedHour, _selectedMinute);
        notificationHandler.ScheduleNotification("Будильник", "Еще 5 минуточек?", alarmTime);
    }

    private DateTime CalculateAlarmTime(int hour, int minute)
    {
        var now = DateTime.Now;
        var alarmTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        if (alarmTime <= now)
        {
            alarmTime = alarmTime.AddDays(1);
        }

        return alarmTime;
    }

    private void SaveAlarm()
    {
        PlayerPrefs.SetInt(AlarmHourKey, _selectedHour);
        PlayerPrefs.SetInt(AlarmMinuteKey, _selectedMinute);
        PlayerPrefs.Save();
    }

    private void LoadSavedAlarm()
    {
        if (!PlayerPrefs.HasKey(AlarmHourKey) || !PlayerPrefs.HasKey(AlarmMinuteKey)) return;

        _selectedHour = PlayerPrefs.GetInt(AlarmHourKey);
        _selectedMinute = PlayerPrefs.GetInt(AlarmMinuteKey);
        dropdownController.UpdateDropdowns(_selectedHour, _selectedMinute);
        UpdateHands();
    }
}