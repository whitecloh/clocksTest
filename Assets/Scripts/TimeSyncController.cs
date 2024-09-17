using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TimeSyncController : MonoBehaviour
{
    [SerializeField] private ClockController clockController;

    private const string TimeApiIoUrl = "https://timeapi.io/api/Time/current/zone?timeZone=Europe/Moscow";
    private const string WorldTimeApiUrl = "https://worldtimeapi.org/api/timezone/Europe/Moscow";

    private DateTime _currentTime;
    private bool _isTimeSynced = false;
    private Coroutine _clockUpdateCoroutine;

    private void Start()
    {
        StartCoroutine(SyncTimeRoutine());
        InvokeRepeating(nameof(SyncTime), 3600f, 3600f);
    }

    private void SyncTime()
    {
        StartCoroutine(SyncTimeRoutine());
    }

    // Синхронизация времени
    private IEnumerator SyncTimeRoutine()
    {
        DateTime? timeFromTimeApiIo = null;
        DateTime? timeFromWorldTimeApi = null;
    
        var timeApiIoRequest = GetTimeFromTimeApiIo(result => timeFromTimeApiIo = result);
        var worldTimeApiRequest = GetTimeFromWorldTimeApi(result => timeFromWorldTimeApi = result);

        StartCoroutine(timeApiIoRequest);
        StartCoroutine(worldTimeApiRequest);

        const float timeout = 5f;
        var startTime = Time.time;

        while (timeFromTimeApiIo == null || timeFromWorldTimeApi == null && Time.time - startTime < timeout)
        {
            yield return null;
        }
        
        var selectedTime = SelectBestTime(timeFromTimeApiIo, timeFromWorldTimeApi, DateTime.Now);

        if (selectedTime != null)
        {
            _currentTime = selectedTime.Value;
            _isTimeSynced = true;
        }
        else
        {
            _currentTime = DateTime.Now;
            _isTimeSynced = true;
        }
        
        _clockUpdateCoroutine ??= StartCoroutine(UpdateClockEverySecond());
    }

    // Выбор более точного времени
    private static DateTime? SelectBestTime(DateTime? time1, DateTime? time2, DateTime currentTime)
    {
        if (time1 == null && time2 == null) return null;
        if (time1 == null) return time2;
        if (time2 == null) return time1;

        var diff1 = (time1.Value - currentTime).Duration();
        var diff2 = (time2.Value - currentTime).Duration();

        return diff1 <= diff2 ? time1 : time2;
    }

    // Запрос времени с 1 источника
    private static IEnumerator GetTimeFromTimeApiIo(Action<DateTime?> onTimeReceived)
    {
        using var webRequest = UnityWebRequest.Get(TimeApiIoUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            onTimeReceived?.Invoke(null);
        }
        else
        {
            var json = webRequest.downloadHandler.text;
            var timeApiIoTime = ParseTimeFromTimeApiIoJson(json);
            
            onTimeReceived?.Invoke(timeApiIoTime);
        }
    }
    
    // Запрос времени с 2 источника
    private static IEnumerator GetTimeFromWorldTimeApi(Action<DateTime?> onTimeReceived)
    {
        using var webRequest = UnityWebRequest.Get(WorldTimeApiUrl);
        yield return webRequest.SendWebRequest();

        if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            onTimeReceived?.Invoke(null);
        }
        else
        {
            var json = webRequest.downloadHandler.text;
            var worldTime = ParseTimeFromJson(json);
            
            onTimeReceived?.Invoke(worldTime);
        }
    }
    
    // Парсим данные с 1 источника
    private static DateTime? ParseTimeFromTimeApiIoJson(string json)
    {
        try
        {
            var jsonObject = JsonUtility.FromJson<TimeApiIoResponse>(json);
            return DateTime.Parse(jsonObject.dateTime);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    // Парсим данные с 2 источника
    private static DateTime? ParseTimeFromJson(string json)
    {
        try
        {
            var jsonObject = JsonUtility.FromJson<WorldTimeApiResponse>(json);
            return DateTime.Parse(jsonObject.datetime);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    // Коррутина обновления времени
    private IEnumerator UpdateClockEverySecond()
    {
        while (_isTimeSynced)
        {
            yield return new WaitForSeconds(1f);

            _currentTime = _currentTime.AddSeconds(1);
            clockController.UpdateClock(_currentTime.Hour, _currentTime.Minute, _currentTime.Second);
        }
    }

    [Serializable]
    private class TimeApiIoResponse
    {
        public string dateTime;
    }

    [Serializable]
    private class WorldTimeApiResponse
    {
        public string datetime;
    }
}