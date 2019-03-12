using UnityEngine;

public class SpeedForce : MonoBehaviour
{
    private TimeManager timeManager;
    // Start is called before the first frame update
    void Start()
    {
        timeManager = TimeManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            timeManager.ToggleTime();
        }
    }
}
