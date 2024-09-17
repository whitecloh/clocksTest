using UnityEngine;
using UnityEngine.UI;

    public class ClockController : MonoBehaviour
    {
        [SerializeField] private ClockHand hourHand;
        [SerializeField] private ClockHand minuteHand;
        [SerializeField] private ClockHand secondHand;
        [SerializeField] private Text digitalClock;

        public void UpdateClock(int hour, int minute, int second)
        {
            var hourAngle = (360f / 12f) * (hour % 12);
            var minuteAngle = (360f / 60f) * minute;
            var secondAngle = (360f / 60f) * second;

            hourHand.UpdateHand(hourAngle);
            minuteHand.UpdateHand(minuteAngle);
            secondHand.UpdateHand(secondAngle);

            digitalClock.text = $"{hour:00}:{minute:00}:{second:00}";
        }
    }
