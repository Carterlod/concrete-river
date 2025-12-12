using System.Collections;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField]
    FishConfig config;

    [SerializeField]
    UnityEngine.AI.NavMeshAgent thisAgent;

    bool isMoving;
    
    void Start()
    {
        isMoving = true;
        StartCoroutine(Update());
    }

    IEnumerator Update()
    {
        while (true){
            float offsetAngle = Random.Range(-config.repathAngle, config.repathAngle);
            thisAgent.SetDestination(transform.position + Quaternion.AngleAxis(offsetAngle,Vector3.up)* transform.forward * 10f);
            yield return new WaitForSeconds(config.repathTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "sound")
        {
            ScareMe(other.gameObject.transform);
        }
    }

    public void ScareMe(Transform loudNoiseSource)
    {
        StopAllCoroutines();
        StartCoroutine(Run(loudNoiseSource)); 
    }

    IEnumerator Run(Transform runFrom)
    {
        while (true)
        {
            Vector3 runDirection = transform.position - runFrom.position;
            runDirection.Normalize();
            thisAgent.SetDestination(transform.position + runDirection * 10f);
            yield return new WaitForSeconds(config.repathTime);
            StartCoroutine(Update());
            yield return null;
        }
    }

    IEnumerator KillAfterDuration()
    {
        yield return new WaitForSeconds(config.lifetime);
        Destroy(this.gameObject);
    }
    public void StopMoving()
    {
        isMoving = false;
        thisAgent.enabled = false;
    }
}
