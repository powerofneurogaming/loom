using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CubeSpawner : Singleton<CubeSpawner>
{
    // Start is called before the first frame update
    bool started = false;
    void Start()
    {
        
    }

    public Material red;
    public Material blue;
    public Material yellow;
    public Material white;

    public GameObject redCube;
    public GameObject blueCube;
    public GameObject yellowCube;
    public GameObject whiteCube;

    public GameObject cube;

    float spawnDistance = 3f;
    public List<GameObject> CubeDrops = new List<GameObject>();
    public bool dropping = false;
    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Takes a color and adds a cube, matching the color, to CubeDrops.
    /// </summary>
    /// <param name="color"> The color cube we want. </param>
    public void AddCube(Color color)
    {
        if (color == Color.red) { CubeDrops.Add(redCube); }
        else if (color == Color.blue) { CubeDrops.Add(blueCube); }
        else if (color == Color.yellow) { CubeDrops.Add(yellowCube); }
        else { CubeDrops.Add(whiteCube); }

    }
    /// <summary>
    /// Starts the coroutine and calls the DropBlocks function.
    /// </summary>
    /// <param name="interval"> Time between cube drops. </param>
    /// <param name="speed"> Speed of of falling blocks. </param>
    public void dropCubes(float interval, float speed)
    {
        StartCoroutine(DropBlocks(CubeDrops, interval, speed));
    }

    /// <summary>
    /// Takes the populated list of cubes and drops them at the specified time interval and speed
    /// </summary>
    /// <param name="cubes"></param>
    /// <param name="interval"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    IEnumerator DropBlocks(List<GameObject> cubes, float interval, float speed)
    {
        while (cubes.Count > 0)
        {
            int RandInt = Random.Range(0, cubes.Count);
            //int RandInt = 0;
            GameObject newCube = Instantiate(cubes[RandInt], new Vector3(0, 12, Random.Range(-3f, 3f)), Quaternion.identity);
            newCube.GetComponent<CubeBehavior>().speed = speed;
            cubes.RemoveAt(RandInt);
            cubes.TrimExcess();
            yield return new WaitForSeconds(interval); //diffulty change here
        }
    }

}
