using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;


public class GameManager : MonoBehaviour
{
    [SerializeField] AudioSource as_music;
    private bool musicMuted = false;
    private float originalMusicVolume = 1;
    [SerializeField] TMP_Text mutedText;


    private float timer = 0;
    private bool uiOn = true;
    [SerializeField] float idleTimeBeforeUIEnabled = 2f;
    [SerializeField] GameObject uiParent;

    

    bool AnyNonButtonInput()
    {
        foreach(var device in InputSystem.devices)
        {
            if(device is Gamepad gamepad)
            {
                if (gamepad.leftStick.ReadValue().magnitude > 0.1f) return true;
                if (gamepad.rightStick.ReadValue().magnitude > 0.1f) return true;
            }
            
        }
        return false;
    }
    private void OnEnable()
    {
        InputSystem.onAnyButtonPress.Call(_ => ResetIdle());
    }
    private void Start()
    {
        originalMusicVolume = as_music.volume;
        mutedText.enabled = false;
    }

    private void ResetIdle()
    {
        timer = 0;
        if (uiOn)
        {
            uiParent.SetActive(false);
            uiOn = false;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!musicMuted)
            {
                as_music.volume = 0;
                musicMuted = true;
                mutedText.text = "music muted";
                StopAllCoroutines();
                StartCoroutine(C_RevealText());
            }
            else
            {
                as_music.volume = originalMusicVolume;
                musicMuted = false;
                mutedText.text = "music playing";
                StopAllCoroutines();
                StartCoroutine(C_RevealText());
            }
        }
        timer += Time.deltaTime;

        if (AnyNonButtonInput())
        {
            ResetIdle();
        }
        
        if(!uiOn && timer > idleTimeBeforeUIEnabled)
        {
            uiParent.SetActive(true);
            uiOn = true;
        }


       
    }



    

    IEnumerator C_RevealText()
    {
        mutedText.enabled = true;
        yield return new WaitForSeconds(1f);
        mutedText.enabled = false;
        yield return null;
    }
}
