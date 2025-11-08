using UnityEngine;
using UnityEngine.InputSystem;

public class HeronController : MonoBehaviour
{
    InputAction moveBodyAction;
    InputAction moveHeadAction;
    [SerializeField] Transform root;
    [SerializeField] float bodyMovementSpeed = 1;
    [SerializeField] float bodyRotationSpeed = 1;
    [SerializeField] Transform head;
    Vector3 initialHeadPos;
    [SerializeField] float headMovementRange = 1f;
    
    [SerializeField] float headMovementSpeed = 1;
    [SerializeField] Camera cam;
    [SerializeField] float cameraHeadingOffsetFromHead = 1;

    private void Start()
    {
        moveBodyAction = InputSystem.actions.FindAction("Move");
        moveHeadAction = InputSystem.actions.FindAction("Look");
        initialHeadPos = head.localPosition;
    }
    void Update()
    {
        //Body movement
        Vector2 moveValue = moveBodyAction.ReadValue<Vector2>();
        root.position += root.forward * moveValue.y * bodyMovementSpeed * Time.deltaTime;
        root.Rotate(0, moveValue.x * bodyRotationSpeed, 0);

        // Head movement
        Vector2 moveHeadValue = moveHeadAction.ReadValue<Vector2>();
        Vector3 headTargetPos = initialHeadPos;
        headTargetPos.y += moveHeadValue.y * headMovementRange;
        head.transform.localPosition = headTargetPos;

        //Camera positioning
        Vector3 headOffset = head.position - head.right * cameraHeadingOffsetFromHead;
        cam.transform.LookAt(headOffset);
    }
}
