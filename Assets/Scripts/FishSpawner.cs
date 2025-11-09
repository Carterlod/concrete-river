using System.Collections;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] GameObject fishPrefab;
    [SerializeField] float spawnInterval = 2f;
    
    [SerializeField] int initialFishCount = 5;
    
    void Start()
    {
        StartCoroutine(SpawnFish());
    }

    IEnumerator SpawnFish()
    {
        for (int i = 0; i < initialFishCount; i++){
            SpawnSingleFish();
        }
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSingleFish();
        }
    }
    void SpawnSingleFish()
    {
       var newFish = Instantiate(fishPrefab, transform.position, Quaternion.AngleAxis(Random.Range(0,360), Vector3.up));
       newFish.transform.SetParent(transform);
    }
}
