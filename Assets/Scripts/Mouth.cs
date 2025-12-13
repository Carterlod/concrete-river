using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class Mouth : MonoBehaviour
{
    [SerializeField] ParticleSystem splashPS;
    HeronController heronController;
    [SerializeField] AudioSource _as;
    [SerializeField] private AudioClip[] splashClip;
    [SerializeField] private AudioClip skewerClip;
    [SerializeField] private NoiseMaker noiseCollider;
    private void Start()
    {
        heronController = GetComponentInParent<HeronController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("water collision");
        splashPS.gameObject.transform.position = collision.GetContact(0).point;
        splashPS.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "water")
        {
            splashPS.gameObject.transform.position = other.ClosestPoint(transform.position);
            splashPS.Play();
            _as.PlayOneShot(splashClip[Random.Range(0, splashClip.Length)]);
            noiseCollider.PingNoise(transform.position, 3f);
        }

        var foundFish = other.GetComponentInParent<FishController>();
        if(foundFish!=null && isSnapping){
            heronController.GrabFish(foundFish);
            _as.PlayOneShot(skewerClip);
        }
    }
    void OnTriggerStay( Collider other )
    {

    }
    public bool isSnapping;

    IEnumerator C_PingNoise()
    {
        noiseCollider.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        noiseCollider.gameObject.SetActive(false);
        yield return null;
    }
}
