using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static Unity.VisualScripting.Member;

public class HeronController : MonoBehaviour
{

    [Header("Body & Movement")]
    InputAction moveBodyAction;
    InputAction moveHeadAction;
    InputAction shoulderButtonRightAction;
    InputAction shoulderButtonLeftAction;
    [SerializeField] Transform root;
    [SerializeField] Transform head;
    [SerializeField] Transform headRestPos;
    [SerializeField] Vector3 headMovePose;

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

    [Header("Animation")]
    [SerializeField] Coroutine callRoutine;
    private bool cooldownActive = false;

    [FormerlySerializedAs("_as")] [Header("Audio")] [SerializeField] private AudioSource as_oneshots;
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

    
    
    
    public void GrabFish(FishController fishController)
    {
        if(hasFish){
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
        jawPivot.localEulerAngles = new Vector3(currentJawX, jawPivot.localEulerAngles.y,jawPivot.localEulerAngles.z);
        float SnapValue()
        {
            return shoulderButtonRightAction.ReadValue<float>();
        }
        
        for (var i = 0; i < 3; i++){

            while (SnapValue() > 0.5f){
                yield return null;
            }


            while (SnapValue() <= 0.5f){
                yield return null;
            }
            as_oneshots.PlayOneShot(gulpClip);
            fishController.transform.localPosition = Vector3.Lerp(Vector3.zero, heldFishFinalPos.localPosition, (i + 1) / 3f);
        }
        
        Instantiate(eatParticleSystem, heldFishPos.position, Quaternion.identity);
        Destroy(fishController.gameObject);
        jawPivot.localEulerAngles = new Vector3(startingJawY, jawPivot.localEulerAngles.y,jawPivot.localEulerAngles.z);
        as_looping.Stop();
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
        Vector2 moveValue = moveBodyAction.ReadValue<Vector2>();
        // Head rotation
        Vector2 moveHeadValue = moveHeadAction.ReadValue<Vector2>();
        smoothedHeadInput = Damp(smoothedHeadInput, moveHeadValue, config.smoothedLookLambda, Time.deltaTime);
        head.localRotation = Quaternion.Euler(80 * -smoothedHeadInput.y,90 * moveHeadValue.x,0f);

        //Camera positioning
        Vector3 headOffset = cameraGoal.position - cameraGoal.right * config.cameraHeadingOffsetFromHead;
        cam.transform.LookAt(headOffset);
        Vector3 newCamPos = initialCamPositionLocal;
        newCamPos.x = initialCamPositionLocal.x + 5f * -moveHeadValue.x;
        newCamPos.y = initialCamPositionLocal.y + 5f * -moveHeadValue.y;
        if(newCamPos.y < 0)
        {
            newCamPos.y = 0;
        }


        cam.transform.localPosition = Damp(cam.transform.localPosition, newCamPos, config.camBodyMovementSpeed, Time.deltaTime);

        //Camera FoV
        float fovRange = Mathf.InverseLerp(-1, 1, moveHeadValue.y);
        fovRange = config.camFoVCurve.Evaluate(fovRange);
        smoothedFovRange = Mathf.Lerp(smoothedFovRange, fovRange, Time.deltaTime * config.fovSmoothTime);
        cam.fieldOfView = Mathf.LerpUnclamped(config.FOVMinMax.x, config.FOVMinMax.y, smoothedFovRange);

        //Head move
        headMovePose = headRestPos.position;
        //headMovePose.x += 0.5f * moveValue.x ;
        headMovePose += head.right * 0.5f * moveValue.x;
        //headMovePose.y += 0.5f * moveValue.y;
        headMovePose += head.up * 0.5f * moveValue.y;
        //head.position = headMovePose;

        //Head snap
        float snapValue = shoulderButtonRightAction.ReadValue<float>();
        
        headMovePose += head.forward * config.headSnapDistance * snapValue;
        //Vector3 snapTargetPos = headRestPos.localPosition + head.forward;
        head.position = headMovePose;
        if(snapValue > 0 && snapValueLastFrame == 0)
        {
            //callRoutine = StartCoroutine(C_Call());
            as_oneshots.PlayOneShot(stabClip);
        }


        if(snapValue - snapValueLastFrame > 0f){
            mouthController.isSnapping = true;
        }
        else{
            mouthController.isSnapping = false;
        }
        
        
        snapValueLastFrame = snapValue;
        

        //Stepping
        float stepValue = shoulderButtonLeftAction.ReadValue<float>();
        if (stepValue == 1 && stepValueLastFrame < 1 && !stepping)
        {
            Debug.Log("Step conditions cleared");
            RaycastHit hit; 
            if(Physics.Raycast(head.position, head.forward, out hit, 10, LayerMask.GetMask("Ground")))
            {
                Debug.Log("hit something");
                
                StartCoroutine(C_Move(hit.point));
            }
        }
        
    }

    IEnumerator C_Move(Vector3 stepPos)
    {
        stepping = true;
        Vector3 startPos = root.position;
        float distanceToGoal = Vector3.Distance(startPos, stepPos);
        float t = 0;
        bodyRotTarget.LookAt(stepPos);
        
        while(distanceToGoal > 0.1f)
        {
            t += Time.deltaTime;
            bodyRotTarget.position = root.position;
            //Debug.Log("distance to goal = " + distanceToGoal);
            //Vector3 targetDirection = stepPos - root.position;
            //targetDirection.x = 0;
            //Quaternion targetRot = Quaternion.LookRotation(targetDirection);
            root.rotation = Quaternion.Lerp(root.rotation, bodyRotTarget.rotation, t * config.bodyRotationSpeed);

            root.position = Vector3.Lerp(startPos, stepPos, t* config.bodyMovementSpeed);
            distanceToGoal = Vector3.Distance(root.position, stepPos);
            yield return null;
        }
        stepping = false;
        yield return null;
    }

    IEnumerator C_Call()
    {
        cooldownActive = true;
        float t = 0;
        float d = 0.5f;
        while(t < d)
        {
            t += Time.deltaTime;
            yield return null ;
        }
        cooldownActive = false;
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
