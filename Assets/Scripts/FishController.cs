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



        // transform.position += transform.forward * config.movementSpeed * Time.deltaTime;
    }
    public void StopMoving()
    {
        isMoving = false;
        thisAgent.enabled = false;
    }
}
