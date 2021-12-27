using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerSyst : MonoBehaviour
{
    public class TimerEvent
    {
        private Action callback;

        private bool isValid;

        public bool IsValid => IsValid;

        public void CreateTimer(Action newCallback)
        {
            callback = newCallback;
            isValid = true;
        }

        public void StopTimer()
        {
            isValid = false;
        }

        public void Execute()
        {
            callback?.Invoke();
            StopTimer();
        }
    }

    private static TimerSyst instance;

    private void Awake()
    {
        instance = this;
    }

    public static TimerEvent CreateTimer(float timeToWait, Action callback = null)
    {
        return instance.OnCreateTimer(timeToWait, callback);
    }

    private TimerEvent OnCreateTimer(float timeToWait, Action callback)
    {
        TimerEvent toReturn = new TimerEvent();
        toReturn.CreateTimer(callback);

        StartCoroutine(TimerHandler(toReturn, timeToWait));

        return toReturn;
    }

    IEnumerator TimerHandler (TimerEvent timer, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        timer?.Execute();
    }
}
