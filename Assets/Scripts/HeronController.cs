using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static Unity.VisualScripting.Member;


// TO DO 
// move with left stick by default, controls head when left trigger is held
// walk animation


public class HeronController : MonoBehaviour
{

    [Header("Body & Movement")]
    InputAction moveBodyAction;
    InputAction moveHeadAction;
    InputAction shoulderButtonRightAction;
    InputAction shoulderButtonLeftAction;
    [SerializeField] Transform root;
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform head;
    [SerializeField] Transform headRestPos;
    [SerializeField] Transform crouchHeadRestPos;
    [SerializeField] Vector3 headMovePose;
    public Vector2 moveValue = Vector2.zero;

    [SerializeField]
    Transform heldFishPos;

    [SerializeField]
    Transform heldFishFinalPos;

    private float snapValueLastFrame = 0;
    [SerializeField] bool stepping = false;
    [SerializeField] Transform bodyRotTarget;
    private float stepValueLastFrame = 0;

    [Header("Camera")]
    [SerializeField] Camera cam;
    private Vector3 initialCamPositionLocal;
    [SerializeField] Transform cameraGoal;
    private float smoothedFovRange = 0;
    [SerializeField] Transform camLookAtBase;

    [Header("Animation")]
    [SerializeField] Coroutine callRoutine;
    private bool cooldownActive = false;
    [SerializeField] Animator animator;

    [FormerlySerializedAs("_as")][Header("Audio")][SerializeField] private AudioSource as_oneshots;
    [SerializeField] private AudioSource as_looping;
    [SerializeField] private AudioClip stabClip;
    [SerializeField] private AudioClip gulpClip;
    [SerializeField] private AudioClip fishFlopClip;

    [Header("Other")]

    [SerializeField]
    Mouth mouthController;

    [SerializeField]
    GameObject eatParticleSystem;

    [SerializeField] private HeronConfig config;
    bool hasFish = false;

    [SerializeField]
    Transform jawPivot;

    [SerializeField]
    private HeronImageSpawner PS_herons;


    public void GrabFish(FishController fishController)
    {
        if (hasFish)
        {
            return;
        }
        StartCoroutine(HandleGrabbedFish(fishController));
    }

    IEnumerator HandleGrabbedFish(FishController fishController)
    {
        fishController.StopMoving();
        hasFish = true;
        fishController.transform.SetParent(heldFishPos);
        fishController.transform.localPosition = Vector3.zero;
        fishController.transform.localRotation = Quaternion.identity;
        float startingJawY = jawPivot.localRotation.x;
        float currentJawX = config.openJawAngle;
        as_looping.Play();
        jawPivot.localEulerAngles = new Vector3(currentJawX, jawPivot.localEulerAngles.y, jawPivot.localEulerAngles.z);
        float SnapValue()
        {
            return shoulderButtonRightAction.ReadValue<float>();
        }

        for (var i = 0; i < 3; i++)
        {

            while (SnapValue() > 0.5f)
            {
                yield return null;
            }

            while (SnapValue() <= 0.5f)
            {
                yield return null;
            }
            as_oneshots.PlayOneShot(gulpClip);
            fishController.transform.localPosition = Vector3.Lerp(Vector3.zero, heldFishFinalPos.localPosition, (i + 1) / 3f);
        }

        //Instantiate(eatParticleSystem, heldFishPos.position, Quaternion.identity);
        Destroy(fishController.gameObject);
        jawPivot.localEulerAngles = new Vector3(startingJawY, jawPivot.localEulerAngles.y, jawPivot.localEulerAngles.z);
        as_looping.Stop();
        PS_herons.spawnArt();
        hasFish = false;
    }

    Vector2 smoothedHeadInput;

    private void Start()
    {
        var inputActions = InputSystem.actions;
        inputActions.Enable();
        moveBodyAction = inputActions.FindAction("Move");
        moveHeadAction = inputActions.FindAction("Look");
        shoulderButtonRightAction = inputActions.FindAction("Snap");
        shoulderButtonLeftAction = inputActions.FindAction("Step");
        initialCamPositionLocal = cam.transform.localPosition;
    }
    void Update()
    {
        //Body movement
        float leftTriggerValue = shoulderButtonLeftAction.ReadValue<float>();
        moveValue = moveBodyAction.ReadValue<Vector2>();
        animator.SetBool("walking", false);
        if (leftTriggerValue == 0)
        {
            headMovePose = headRestPos.position;
            characterController.Move(root.forward * moveValue.y * Time.deltaTime * config.bodyMovementSpeed);
            characterController.gameObject.transform.Rotate(Vector3.up, config.bodyRotationSpeed * Time.deltaTime * smoothedHeadInput.x);
            if (moveValue.x > 0 || moveValue.y > 0) 
            { 
                animator.SetBool("walking", true); 
                animator.speed = moveValue.y;
            }
        }
        else if (leftTriggerValue > 0)
        {
            //Head move
            headMovePose = crouchHeadRestPos.position;
            headMovePose += root.right * 0.5f * moveValue.x;
            if (moveValue.y > 0)
            {
                headMovePose += head.up * 0.5f * moveValue.y; 
            }
            else
            {
                headMovePose += root.up * 0.5f * moveValue.y;
            }
        }

        // Head rotation
        Vector2 moveHeadValue = moveHeadAction.ReadValue<Vector2>();
        smoothedHeadInput = Damp(smoothedHeadInput, moveHeadValue, config.smoothedLookLambda, Time.deltaTime);
        head.localRotation = Quaternion.Euler(55 * -smoothedHeadInput.y + 10 * leftTriggerValue, 120 * moveHeadValue.x, 0f); 

        //Camera rotation
        Vector3 headOffset = cameraGoal.position - cameraGoal.right * config.cameraHeadingOffsetFromHead;
        camLookAtBase.position = cam.transform.position;
        camLookAtBase.LookAt(headOffset);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, camLookAtBase.rotation, Time.deltaTime * 5);

        //Camera position
        Vector3 newCamPos = initialCamPositionLocal;
        newCamPos.x = initialCamPositionLocal.x + 5f * -moveHeadValue.x;
        newCamPos.y = initialCamPositionLocal.y + 5f * -moveHeadValue.y;
        if (newCamPos.y < 0)
        {
            newCamPos.y = 0;
        }

        //CameraZoom
        if(leftTriggerValue > 0)
        {
            newCamPos += cam.transform.forward * 1.5f;
        }

        cam.transform.localPosition = Damp(cam.transform.localPosition, newCamPos, config.camBodyMovementSpeed, Time.deltaTime);

        //Camera FoV
        float fovRange = Mathf.InverseLerp(-1, 1, moveHeadValue.y);
        fovRange = config.camFoVCurve.Evaluate(fovRange);
        smoothedFovRange = Mathf.Lerp(smoothedFovRange, fovRange, Time.deltaTime * config.fovSmoothTime);

        cam.fieldOfView = Mathf.LerpUnclamped(config.FOVMinMax.x, config.FOVMinMax.y, smoothedFovRange);
        


        //Peck
        float rightTriggerValue = shoulderButtonRightAction.ReadValue<float>();
        Vector3 snapTargetPos = headRestPos.localPosition + head.forward;
        if (rightTriggerValue > 0)
        {
            headMovePose += head.forward * config.headSnapDistance * rightTriggerValue;
            head.position = headMovePose;
        }
        else
        {
            head.position = Vector3.Lerp(head.position, headMovePose, Time.deltaTime * 10);
        }
        if (rightTriggerValue > 0 && snapValueLastFrame == 0)
        {
            as_oneshots.PlayOneShot(stabClip);
        }


        if (rightTriggerValue - snapValueLastFrame > 0f)
        {
            mouthController.isSnapping = true;
        }
        else
        {
            mouthController.isSnapping = false;
        }
        snapValueLastFrame = rightTriggerValue;

        //gravity
        characterController.Move(Vector3.down * 9.81f);
    }

    public static float Damp(float a, float b, float lambda, float dt)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }
    public static Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
    {
        return Vector3.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }
    public static Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
    {
        return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }
    public static Vector2 Damp(Vector2 a, Vector2 b, float lambda, float dt)
    {
        return Vector2.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

}
