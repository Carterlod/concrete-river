using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [SerializeField] AudioSource aSource;
    [SerializeField] AudioClip[] footstepsWater;
    private float startingVolume = 0;
    private void Start()
    {
        startingVolume = aSource.volume;
    }
    public void PlayFootstep(float volume)
    {
        float newPitch = Random.Range(0.5f, 1.2f);
        aSource.volume = startingVolume * volume;
        aSource.pitch = newPitch; 
        aSource.PlayOneShot(footstepsWater[Random.Range(0, footstepsWater.Length - 1)]);
    }
}
