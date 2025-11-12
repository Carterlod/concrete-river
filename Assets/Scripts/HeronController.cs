using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using static Unity.VisualScripting.Member;

public class HeronController : MonoBehaviour
{

    [Header("Body & Movement")]
    InputAction moveAction1;
    InputAction moveAction2;
    InputAction shoulderButtonRightAction;
    InputAction shoulderButtonLeftAction;
    [SerializeField] Transform root;
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform head;
    [SerializeField] Transform headRestPos;
    [SerializeField] Vector3 headMovePose;
    [SerializeField] Transform stepPositionOrigin;
    [SerializeField] Transform stepPositionDestination;
    [SerializeField] Transform stepPositionProgress;
    [SerializeField] Transform stepEndRotation;
 
    [SerializeField]
    Transform heldFishPos;

    [SerializeField]
    Transform heldFishFinalPos;

    private float snapValueLastFrame = 0;
    [SerializeField] bool stepping = false;
    [SerializeField] Transform bodyRotTarget;
    private float stepValueLastFrame = 0;
    [SerializeField] float stepValue = 0f;
    [SerializeField] bool stepCommitted = false;

    [Header("Camera")]
    [SerializeField] Camera cam;
    private Vector3 initialCamPositionLocal;
    [SerializeField] Transform cameraGoal;
    private float smoothedFovRange = 0;
    [SerializeField] Transform camLookAtBase;
    [SerializeField] Transform camBodyTarget;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] Coroutine callRoutine;
    private bool cooldownActive = false;

    [FormerlySerializedAs("_as")] [Header("Audio")] [SerializeField] private AudioSource as_oneshots;
    [SerializeField] private AudioSource as_looping;
    [SerializeField] private AudioClip stabClip;
    [SerializeField] private AudioClip gulpClip;
    [SerializeField] private AudioClip fishFlopClip;

    [Header("Particles")]
    [SerializeField] ParticleSystem rippleFX;

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
        moveAction1 = inputActions.FindAction("Move");
        moveAction2 = inputActions.FindAction("Look");
        shoulderButtonRightAction = inputActions.FindAction("Snap");
        shoulderButtonLeftAction = inputActions.FindAction("Step");
        initialCamPositionLocal = camBodyTarget.transform.localPosition;
        smoothedStepProgress = characterController.transform.position;
    }
    [SerializeField] Vector3 smoothedStepProgress;
    [SerializeField] Vector3 destinationProgress;
    Quaternion stepStartRotation;
    float smoothedRotationAmount;
    void Update()
    {
        //Body movement
        Vector2 moveValue = moveAction1.ReadValue<Vector2>();
        // Head rotation
        Vector2 moveHeadValue = moveAction2.ReadValue<Vector2>();
        smoothedHeadInput = Damp(smoothedHeadInput, moveHeadValue, config.smoothedLookLambda, Time.deltaTime);
        head.localRotation = Quaternion.Euler(55 * -smoothedHeadInput.y, 50 * moveHeadValue.x, 0f);

        //Camera positioning
        Vector3 headOffset = cameraGoal.position - cameraGoal.right * config.cameraHeadingOffsetFromHead;
        Vector3 headMoveDelta = headMovePose - headRestPos.position;
        headOffset += headMoveDelta;

        camLookAtBase.localPosition = camBodyTarget.transform.localPosition;
        camLookAtBase.LookAt(headOffset);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, camLookAtBase.rotation, Time.deltaTime * 5);
        Vector3 newCamPos = initialCamPositionLocal;
        newCamPos.x = initialCamPositionLocal.x + 5f * -moveHeadValue.x;
        newCamPos.y = initialCamPositionLocal.y + 5f * -moveHeadValue.y;
        if(newCamPos.y < 0)
        {
            newCamPos.y = 0;
        }


        camBodyTarget.localPosition = newCamPos;
        cam.transform.position = Damp(cam.transform.position, camBodyTarget.position, config.camBodyMovementSpeed, Time.deltaTime);

        //Camera FoV
        float fovRange = Mathf.InverseLerp(-1, 1, moveHeadValue.y);
        fovRange = config.camFoVCurve.Evaluate(fovRange);
        smoothedFovRange = Mathf.Lerp(smoothedFovRange, fovRange, Time.deltaTime * config.fovSmoothTime);
        cam.fieldOfView = Mathf.LerpUnclamped(config.FOVMinMax.x, config.FOVMinMax.y, smoothedFovRange);

        //Head move
        headMovePose = headRestPos.position;
        headMovePose += root.right * 0.5f * moveValue.x;

        if(moveValue.y > 0)
        {
            headMovePose += head.up * 0.5f * moveValue.y;

        }
        else
        {
            headMovePose += root.up * 0.5f * moveValue.y;

        }
        //head.position = headMovePose;

        //Head snap
        float snapValue = shoulderButtonRightAction.ReadValue<float>();
        Vector3 snapTargetPos = headRestPos.localPosition + head.forward;
        if (snapValue > 0)
        {
            headMovePose += head.forward * config.headSnapDistance * snapValue;
            head.position = headMovePose;
        }
        else
        {
            head.position = Vector3.Lerp(head.position, headMovePose, Time.deltaTime * 10);
        }
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
        stepValue = shoulderButtonLeftAction.ReadValue<float>();
        if(shoulderButtonLeftAction.activeControl == null)
        {
            stepValue = 0f;
        }
        
        characterController.transform.position = smoothedStepProgress;

        if (stepValue > 0)
        {
            animator.SetBool("walking", true);
            Vector3 movementVector = head.forward;
            RaycastHit hit = new RaycastHit();
           
            stepPositionProgress.position = smoothedStepProgress;
            if(stepValueLastFrame == 0)
            {
                stepPositionOrigin.position = characterController.transform.position;
                stepPositionOrigin.forward = characterController.transform.forward;
                stepStartRotation = stepPositionOrigin.rotation;
                //stepPositionOrigin.Rotate(Vector3.up, config.bodyRotationSpeed * Time.deltaTime * smoothedHeadInput.x);
                if (Physics.Raycast(stepPositionOrigin.position + movementVector.normalized * 2f + Vector3.up * 2f, Vector3.down, out hit, 100, LayerMask.GetMask("Ground")))
                {
                    stepPositionDestination.position = hit.point; 
                }
                stepEndRotation.position = characterController.transform.position;
                stepEndRotation.LookAt(hit.point, Vector3.up);
                PlayRippleParticle(characterController.transform.position);
            }
            if (stepValue == 1 && stepCommitted == false)
            {
                stepCommitted = true;
                PlayRippleParticle(hit.point);

            }
            smoothedStepProgress = Vector3.Lerp(smoothedStepProgress, destinationProgress, Time.deltaTime * 4f);
            if (!stepCommitted)
            {
                if (stepValue < 1)
                {
                    smoothedRotationAmount = Mathf.Lerp(smoothedRotationAmount, stepValue, Time.deltaTime * 2f);
                    characterController.transform.localRotation = Quaternion.Lerp(stepStartRotation, stepEndRotation.rotation, config.smoothedStepRotation.Evaluate(stepValue));
                    destinationProgress = Vector3.Slerp(stepPositionOrigin.position, stepPositionDestination.position, stepValue);


                    //characterController.Move(characterController.transform.forward* config.stepDistanceMultiplier * stepValue * Time.deltaTime);
                }
                /*
                else if(stepValue < stepValueLastFrame && stepValue < 1)
                {
                    characterController.Move(-characterController.transform.forward * config.stepDistanceMultiplier * stepValue * Time.deltaTime);
                }
                */


            }
            else if (stepCommitted)
            {
                if(stepValue > stepValueLastFrame)
                {

                }
                if(stepValue < stepValueLastFrame)
                {

                }
            }
            //characterController.gameObject.transform.Rotate(Vector3.up, config.bodyRotationSpeed * Time.deltaTime * smoothedHeadInput.x);
        }
        else if(stepValue == 0 && stepCommitted)
        {
            stepCommitted = false;
            animator.SetBool("walking", false);

        }
        else
        {
            characterController.Move(Vector3.down * 9.81f);
        }
            stepValueLastFrame = stepValue;
        /*
        if (stepValue == 1 && stepValueLastFrame < 1 && !stepping)
        {
            //Vector3 movementVector = root.forward + head.forward;
            characterController.Move(root.forward * stepValue * Time.deltaTime * config.bodyMovementSpeed);
            characterController.gameObject.transform.Rotate(Vector3.up, config.bodyRotationSpeed * Time.deltaTime * smoothedHeadInput.x);
        }
        */
        
        
    }

    IEnumerator C_Move(Vector3 stepPos)
    {
        stepping = true;
        Vector3 startPos = root.position;
        float distanceToGoal = Vector3.Distance(startPos, stepPos);
        float d = 0.5f;
        float t = 0;
        bodyRotTarget.LookAt(stepPos);
        
        while(t < d)
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

    private void PlayRippleParticle(Vector3 pos)
    {
        //rippleFX.transform.position = pos;
        rippleFX.Play();
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
