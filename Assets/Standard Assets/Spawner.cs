using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Vector3 center;
    public Vector3 size;
    public GameObject plusPill;
    public GameObject minusPill;
    public GameObject battery;
    private float time = 0.0f;
    public float interpolationPeriod = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (time >= interpolationPeriod)
        {
            time = 0.0f;

            Spawn();
        }
    }
    public void Spawn()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 2, size.z / 2));
        Vector3 pos2 = center + new Vector3(Random.Range(-size.x / 3, size.x / 3), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 3, size.z / 3));
        Vector3 pos3 = center + new Vector3(Random.Range(-size.x / 4, size.x / 4), Random.Range(-size.y / 2, size.y / 2), Random.Range(-size.z / 4, size.z / 4));
        Instantiate(minusPill, pos, Quaternion.identity);
        Instantiate(plusPill, pos2, Quaternion.identity);
        Instantiate(battery, pos2, Quaternion.identity);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 0, 0);
        Gizmos.DrawCube(center, size);

    }
}
