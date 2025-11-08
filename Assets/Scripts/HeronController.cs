using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] Transform headSnapPos;

    private float snapValueLastFrame = 0;
    private Vector3 initialHeadPos;
    [SerializeField] bool stepping = false;
    [SerializeField] Transform bodyRotTarget;
    private float stepValueLastFrame = 0;

    [Header("Camera")]
    [SerializeField] Camera cam;
    private Vector3 initialCamPositionLocal;
    [SerializeField] Transform cameraGoal;
    private float smoothedFovRange = 0;

    [Header("Animation")]
    [SerializeField] Animator beakAnimator;
    [SerializeField] Coroutine callRoutine;
    private bool cooldownActive = false;

    [Header("Other")]
    [SerializeField] Transform camBodyTargetUp;
    [SerializeField] Transform camBodyTargetDown;
    [SerializeField] Transform camBodyTargetLeft;
    [SerializeField] Transform camBodyTargetRight;


    [SerializeField] private HeronConfig config;


    private void Start()
    {
        var inputActions = InputSystem.actions;
        inputActions.Enable();
        moveBodyAction = inputActions.FindAction("Move");
        moveHeadAction = inputActions.FindAction("Look");
        shoulderButtonRightAction = inputActions.FindAction("Snap");
        shoulderButtonLeftAction = inputActions.FindAction("Step");
        initialCamPositionLocal = cam.transform.localPosition;
        initialHeadPos = head.localPosition;
    }
    void Update()
    {
        //Body movement
        Vector2 moveValue = moveBodyAction.ReadValue<Vector2>();
        /*
        root.position += root.forward * moveValue.y * bodyMovementSpeed * Time.deltaTime;
        root.Rotate(0, moveValue.x * bodyRotationSpeed, 0);
        */

        // Head rotation
        Vector2 moveHeadValue = moveHeadAction.ReadValue<Vector2>();
        Vector3 newHeadDirection = root.forward;
        newHeadDirection.y = root.eulerAngles.y + 90 * moveHeadValue.x;
        newHeadDirection.x = 80 * -moveHeadValue.y;
        head.eulerAngles = newHeadDirection;

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
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, newCamPos, Time.deltaTime * config.camBodyMovementSpeed);

        //Camera FoV
        float fovRange = Mathf.InverseLerp(-1, 1, moveHeadValue.y);
        fovRange = config.camFoVCurve.Evaluate(fovRange);
        smoothedFovRange = Mathf.Lerp(smoothedFovRange, fovRange, Time.deltaTime * config.fovSmoothTime);
        cam.fieldOfView = Mathf.Lerp(config.FOVMinMax.x, config.FOVMinMax.y, smoothedFovRange);

        //Head move
        headMovePose = headRestPos.position;
        //headMovePose.x += 0.5f * moveValue.x ;
        headMovePose += head.right * 0.5f * moveValue.x;
        //headMovePose.y += 0.5f * moveValue.y;
        headMovePose += head.up * 0.5f * moveValue.y;
        head.localPosition = headMovePose;

        //Head snap
        float snapValue = shoulderButtonRightAction.ReadValue<float>();
        //Vector3 snapTargetPos = headRestPos.localPosition + head.forward;
        head.position = Vector3.Lerp(headMovePose, headSnapPos.position, snapValue);
        if(snapValue > 0 && snapValueLastFrame == 0)
        {
            //callRoutine = StartCoroutine(C_Call());
            beakAnimator.SetTrigger("call");
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


}
