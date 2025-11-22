using UnityEngine;

public class bg_particle : MonoBehaviour
{
    
    Vector3 startPos;
    private Transform t_cam;

    public float f;
    public float a;
    public float d;
    public Vector3 v;

    void Start()
    {
        t_cam = GameObject.Find("main cam").transform;
        startPos = transform.position;

        d = Random.Range(-4, 4);
        a = Random.Range(1, 3);
        f = Random.Range(1, 5);

        v = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
    }
    void Update()
    {
        transform.forward = transform.position - t_cam.position;

        transform.position = startPos + v * Mathf.Sin(Time.time * f + d) * a;

        if (Vector3.Distance(startPos, t_cam.position) > 125)
        {
            startPos += (t_cam.position - startPos) * 2;
        }
    }
}
