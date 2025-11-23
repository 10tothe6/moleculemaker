using UnityEngine;

public class Background : MonoBehaviour
{
    public int particleCount;

    public GameObject p_particle;
    public Transform container;

    public int range;
    
    void Start()
    {
        Application.targetFrameRate = 60;
        
        int advance = 1;
        for (int i = 0; i < particleCount; i += advance)
        {
            int numParticles = Mathf.Min(particleCount - i, Random.Range(1, 4));
            advance = numParticles;

            Vector3 dir = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)); 
            float l = 1;
            Vector3 pos = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            for (int j = 0; j < numParticles; j++)
            {
                
                Transform newParticle = Instantiate(p_particle, pos, Quaternion.identity).transform;
                newParticle.SetParent(container);

                newParticle.forward = dir;
                newParticle.Rotate(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)), Random.Range(-10, 10));

                newParticle.position += newParticle.forward * l;
                pos = newParticle.position;
                dir = newParticle.forward;
            }
        }
    }
}
