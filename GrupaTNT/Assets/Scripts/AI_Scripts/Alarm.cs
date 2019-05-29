using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alarm
{
    double timer;
    double timerMax;
    /// <summary>
    /// Timer, needs update function to be called every frame to work.
    /// </summary>
    /// <param name="max">Amount of seconds before timer is considered activ.</param>
    public Alarm(double max) {
        timerMax = max;
        timer = 0;
    }

    public double getTimer() {
        return timer;
    }

    /// <summary>
    /// Increases the timer. Needs to be called every update
    /// </summary>
    public void Update()
    {
        timer += Time.deltaTime;
    }
    /// <summary>
    /// Returns true if time passed exceeded the maximum amount of time set.
    /// </summary>
    /// <returns></returns>
    public bool isActive() {
        if (timer >= timerMax) return true;
        else return false;
    }
    /// <summary>
    /// Resets the timer back to 0
    /// </summary>
    public void reset() {
        timer = 0;
    }
    /// <summary>
    /// Sets the amount of time needed for the alarm to become active. DOES NOT RESET THE ALARM. To reset the alarm use reset().
    /// </summary>
    /// <param name="max"></param>
    public void setMax(double max) {
        timerMax = max;
    }

}
