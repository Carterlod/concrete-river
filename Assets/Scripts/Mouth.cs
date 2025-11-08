using UnityEngine;

public class Mouth : MonoBehaviour
{
    [SerializeField] ParticleSystem splashPS;
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
        }
    }
}
