using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class Mouth : MonoBehaviour
{
    [SerializeField] ParticleSystem splashPS;
    HeronController heronController;
    [SerializeField] AudioSource _as;
    [SerializeField] private AudioClip[] splashClip;
    [SerializeField] private AudioClip skewerClip;
    private void Start()
    {
        heronController = GetComponentInParent<HeronController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("water collision");
        splashPS.gameObject.transform.position = collision.GetContact(0).point;
        splashPS.Play();
        

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "water")
        {
            Debug.Log("water pecked");
            splashPS.gameObject.transform.position = other.ClosestPoint(transform.position);
            splashPS.Play();
            _as.PlayOneShot(splashClip[Random.Range(0, splashClip.Length)]);
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
}
