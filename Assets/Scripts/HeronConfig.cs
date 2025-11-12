using UnityEngine;
[CreateAssetMenu()]
public class HeronConfig : ScriptableObject
{
    public float camBodyMovementSpeed = 5;
    public float bodyMovementSpeed = .5f;
    public float bodyRotationSpeed = .2f;
    public float headMovementSpeed = 1;
    public float headTravelDistanceMax = 1; 
    public AnimationCurve camFoVCurve;
    public float fovSmoothTime = 5;
    public  float cameraHeadingOffsetFromHead = -0.43f;
    public Vector2 FOVMinMax = new Vector2(5, 20);
    public float headSnapDistance;
    public float smoothedLookLambda;
    public float openJawAngle;
    public float smoothedCamLookAtSpeed;
    public float stepDistanceMultiplier;
    public AnimationCurve smoothedStepRotation;

}
