using System.Collections;
using UnityEngine;

public class HeronImageSpawner : MonoBehaviour
{
    private int artIndex = 0;
    [SerializeField] ParticleSystem ps_HeronArt;
    ParticleSystemRenderer psRend;
    [SerializeField] Material[] heronMaterials;
    [SerializeField] AudioClip[] squawks;
    [SerializeField] AudioSource aSource;
    private void Start()
    {
        psRend = ps_HeronArt.GetComponent<ParticleSystemRenderer>();
    }
    public void spawnArt()
    {
        Debug.Log("spawned image");
        artIndex++;
        if(artIndex > heronMaterials.Length)
        {
            artIndex = 0;
        }
        psRend.material = heronMaterials[artIndex];
        ps_HeronArt.Play();
        StartCoroutine(DelayedSquawk());
    }
    IEnumerator DelayedSquawk()
    {
        yield return new WaitForSeconds(0.4f);
        aSource.PlayOneShot(squawks[Random.Range(0, squawks.Length)]);
        yield return null;
    }
}
