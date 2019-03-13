using UnityEngine;

public class BreakFloor : MonoBehaviour
{
    public GameObject breakParticles;
    public float timeScale = .03f;
    public Transform leftPosition;
    public Transform rightPosition;
    // Start is called before the first frame update
    public void LeftFootCrack()
    {
        if(Time.timeScale <= timeScale)
        {
            Instantiate(breakParticles, leftPosition.position, leftPosition.rotation);
        }
    }

    public void RightFootCrack()
    {
        if (Time.timeScale <= timeScale)
        {
            Instantiate(breakParticles, rightPosition.position, rightPosition.rotation);
        }
    }
}
