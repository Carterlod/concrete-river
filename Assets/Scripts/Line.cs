using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;
    [SerializeField] LineRenderer lineR;

    private void Start()
    {
        //lineR = new LineRenderer();
    }
    private void Update()
    {
        lineR.SetPosition(0, startPoint.position);
        lineR.SetPosition(1, endPoint.position);
    }
}
