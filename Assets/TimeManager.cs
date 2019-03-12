using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    #region Singleton
    public static TimeManager instance;

    private void Awake()
    {
        instance = this;
    }
    #endregion

    public float slowDown = .03f;
    public float slowDownSpeed = 1f;

    private bool slowmo = false;

    public void ToggleTime()
    {
        slowmo = !slowmo;
        StopAllCoroutines();
        if (slowmo)
            StartCoroutine(SlowDown());
        else
            StartCoroutine(Resume());
    }

    IEnumerator SlowDown()
    {
        while(Time.timeScale > slowDown)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - Time.unscaledDeltaTime * slowDownSpeed, 0f, 1f);
            Time.fixedDeltaTime = Time.timeScale * 0.01f;
            yield return null;
        }
        Time.timeScale = slowDown;
        Time.fixedDeltaTime = Time.timeScale * .01f;
    }

    IEnumerator Resume()
    {
        while (Time.timeScale < 1f)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale + Time.unscaledDeltaTime * slowDownSpeed, 0f, 1f);
            Time.fixedDeltaTime = Time.timeScale * 0.01f;
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = .01f;
    }




}
