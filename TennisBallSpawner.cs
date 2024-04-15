using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TennisBallSpawner : MonoBehaviour
{
    [SerializeField]
    private int numberOfBallsToSpawn;
    [SerializeField]
    private Vector3 spawnPositionMin;
    [SerializeField]
    private Vector3 spawnPositionMax;
    [SerializeField]
    private GameObject tennisBall;
    [SerializeField]
    private float yInitialMin, yInitialMax;
    [SerializeField]
    private float xInitialMin, xInitialMax;
    List<GameObject> balls = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnBalls());
        
    }

    

    private GameObject SpawnBall()
    {
        GameObject newBall = Instantiate(tennisBall, new Vector3(Random.Range(spawnPositionMin.x, spawnPositionMax.x),
                                                    Random.Range(spawnPositionMin.y, spawnPositionMax.y),
                                                    Random.Range(spawnPositionMin.z, spawnPositionMax.z)), Quaternion.identity);
        newBall.GetComponent<BallController>().yForce = Random.Range(yInitialMin, yInitialMax);
        newBall.GetComponent<BallController>().xForce = Random.Range(xInitialMin, xInitialMax);
        newBall.GetComponent<BallController>().AddForce();
        return newBall;
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    private IEnumerator SpawnBalls()
    {
        while(true)
        {
            if(balls != null && balls.Count > 2)
            {
                for(int i = 0; i < balls.Count; i++)
                {
                    Destroy(balls[i]); 
                }
            }
            for (int i = 0; i < numberOfBallsToSpawn; i++)
            {
                GameObject newBall = SpawnBall();

                balls.Add(newBall);
            }
            yield return new WaitForSeconds(2.5f);
        }

        
    }
}
