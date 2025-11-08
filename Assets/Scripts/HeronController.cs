using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeronController : MonoBehaviour
{
    [Header("Body & Movement")]
    InputAction moveBodyAction;
    InputAction moveHeadAction;
    InputAction shoulderButtonRightAction;
    [SerializeField] Transform root;
    [SerializeField] Transform head;
    [SerializeField] Transform headRestPos;
    [SerializeField] Transform headSnapPos;
    [SerializeField] float bodyMovementSpeed = 1;
    [SerializeField] float bodyRotationSpeed = 1;
    [SerializeField] float headMovementSpeed = 1;
    [SerializeField] float headTravelDistanceMax = 1;
    private Vector3 initialHeadPos;

    [Header("Camera")]
    [SerializeField] Camera cam;
    private Vector3 initialCamPositionLocal;
    [SerializeField] float cameraHeadingOffsetFromHead = 1;
    [SerializeField] Transform cameraGoal;


    [Header("Other")]
    [SerializeField] Transform camBodyTargetUp;
    [SerializeField] Transform camBodyTargetDown;
    [SerializeField] Transform camBodyTargetLeft;
    [SerializeField] Transform camBodyTargetRight;


    private void Start()
    {
        moveBodyAction = InputSystem.actions.FindAction("Move");
        moveHeadAction = InputSystem.actions.FindAction("Look");
        shoulderButtonRightAction = InputSystem.actions.FindAction("Snap");
        initialCamPositionLocal = cam.transform.position;
        //initialHeadPos = head.localPosition;
        //initialHeadRotation = head.rotation;
    }
    void Update()
    {
        //Body movement
        Vector2 moveValue = moveBodyAction.ReadValue<Vector2>();
        root.position += root.forward * moveValue.y * bodyMovementSpeed * Time.deltaTime;
        root.Rotate(0, moveValue.x * bodyRotationSpeed, 0);

        // Head rotation
        Vector2 moveHeadValue = moveHeadAction.ReadValue<Vector2>();
        Vector3 newHeadDirection = root.forward;
        newHeadDirection.y = root.eulerAngles.y + 90 * moveHeadValue.x;
        newHeadDirection.x = 90 * -moveHeadValue.y;
        head.eulerAngles = newHeadDirection;

        //Camera positioning
        Vector3 headOffset = cameraGoal.position - cameraGoal.right * cameraHeadingOffsetFromHead;
        cam.transform.LookAt(headOffset);
        Vector3 newCamPos = initialCamPositionLocal;
        newCamPos.x = initialCamPositionLocal.x + 5f * -moveHeadValue.x;
        newCamPos.y = initialCamPositionLocal.y + 5f * -moveHeadValue.y;
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, newCamPos, Time.deltaTime * 10);


        //Head snap
        float snapValue = shoulderButtonRightAction.ReadValue<float>();
        //Vector3 snapTargetPos = headRestPos.localPosition + head.forward;
        head.position = Vector3.Lerp(headRestPos.position, headSnapPos.position, snapValue);
    }


}
