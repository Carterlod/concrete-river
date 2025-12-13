using UnityEngine;
using System.Collections;

public class NoiseMaker : MonoBehaviour
{
    [SerializeField] private SphereCollider noiseCollider;

    public void PingNoise(Vector3 noiseOrigin, float radius)
    {
        StartCoroutine(C_PingNoise(noiseOrigin, radius));
    }
    IEnumerator C_PingNoise(Vector3 noiseOrigin, float noiseRadius)
    {
        noiseCollider.gameObject.transform.position = noiseOrigin;
        noiseCollider.radius = noiseRadius;
        noiseCollider.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        noiseCollider.gameObject.SetActive(false);
        yield return null;
    }
}
