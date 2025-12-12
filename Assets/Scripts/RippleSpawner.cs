using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RippleSpawner : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] ParticleSystem ps_ripple;
    [SerializeField] HeronController heron;
    [SerializeField] Vector2 sizeRange = new Vector2(5, 15);
    [SerializeField] Vector2 lifetimeRange = new Vector2(1, 10);

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(C_SuppressAtStart());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "water")
        {
            ps_ripple.transform.position = other.ClosestPoint(transform.position);
            var main = ps_ripple.main;
            main.startSize = Mathf.Lerp(sizeRange.x, sizeRange.y, heron.moveValue.y);
            main.startLifetime = Mathf.Lerp(lifetimeRange.x, lifetimeRange.y, heron.moveValue.y);            
            ps_ripple.Play();
        }
    }
    IEnumerator C_SuppressAtStart()
    {
        ps_ripple.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        ps_ripple.gameObject.SetActive(true);
    }
}
