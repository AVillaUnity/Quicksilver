using UnityEngine;

public class BreakFloor : MonoBehaviour
{
    public GameObject breakParticles;
    public float timeScale = .03f;
    public Transform[] footPositions;
    // Start is called before the first frame update
    public void DestroyFloor()
    {
        if(Time.timeScale <= timeScale)
        {
            Vector3 positionToSpawnIn = footPositions[0].position;
            foreach(Transform t in footPositions)
            {
                if(t.position.y < positionToSpawnIn.y)
                {
                    positionToSpawnIn = t.position;
                    break;
                }
            }
            Instantiate(breakParticles, positionToSpawnIn, Quaternion.identity);
        }
    }
}
