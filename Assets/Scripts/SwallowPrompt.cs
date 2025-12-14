using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SwallowPrompt : MonoBehaviour
{
    [SerializeField] Image _asset;
    [SerializeField] Sprite r1;
    [SerializeField] Sprite r2;
    private float t = 0;
    [SerializeField] float duration = 0.25f;

    private void OnEnable()
    {
        t = 0;
    }

    private void Update()
    {
        t += Time.deltaTime;
        if(t > duration)
        {
            t = 0;
        }
        if(t < duration / 2)
        {
            _asset.sprite = r1;
        }
        else
        {
            _asset.sprite = r2;
        }
    }


}
