using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField]
    FishConfig config;

    bool isMoving;
    
    void Start()
    {
        isMoving = true;

    }

    void Update()
    {
        if(!isMoving){
            return;
        }
        transform.position += transform.forward * config.movementSpeed * Time.deltaTime;
    }
    public void StopMoving()
    {
        isMoving = false;
    }
}
