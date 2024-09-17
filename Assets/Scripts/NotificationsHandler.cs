using System;
using Unity.Notifications.Android;
using UnityEngine;

public class NotificationsHandler : MonoBehaviour
{
    private int _id = 0;
    private void Start()
    {
        RequestNotificationPermission();

        var channel = new AndroidNotificationChannel()
        {
            Id = "alarm_channel",
            Name = "Alarm Channel",
            Importance = Importance.High,
            Description = "Channel for alarm notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    // Установки флага на уведомление
    public void ScheduleNotification(string title, string text, DateTime fireTime)
    {
        var notification = new AndroidNotification()
        {
            Title = title,
            Text = text,
            FireTime = fireTime,
        };

        if (_id != 0)
        {
            AndroidNotificationCenter.CancelNotification(_id);
        }

        _id = AndroidNotificationCenter.SendNotification(notification, "alarm_channel");
    }

    //Жесткий запрос отправки уведомлений на андроидах 12-13+
    private void RequestNotificationPermission()
    {
        if (IsAndroidVersion33OrHigher())
        {
            using var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
            var permissionStatus = currentActivity.Call<int>("checkSelfPermission", "android.permission.POST_NOTIFICATIONS");

            if (permissionStatus != 0)
            {
                currentActivity.Call("requestPermissions", new string[] { "android.permission.POST_NOTIFICATIONS" }, 1);
            }
        }

        if (IsAndroidVersion31OrHigher())
        {
            using var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
            
            currentActivity.Call("requestPermissions", new string[] { "android.permission.SCHEDULE_EXACT_ALARM" }, 1);
        }
    }

    private static bool IsAndroidVersion33OrHigher()
    {
        using var version = new AndroidJavaClass("android.os.Build$VERSION");
        var sdkInt = version.GetStatic<int>("SDK_INT");
        return sdkInt >= 33;
    }

    private static bool IsAndroidVersion31OrHigher()
    {
        using var version = new AndroidJavaClass("android.os.Build$VERSION");
        var sdkInt = version.GetStatic<int>("SDK_INT");
        return sdkInt >= 31;
    }
}
