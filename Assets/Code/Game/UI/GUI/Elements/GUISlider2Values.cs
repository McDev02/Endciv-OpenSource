using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUISlider2Values : MonoBehaviour
{
    [SerializeField] RectTransform handle1;
    [SerializeField] RectTransform handle2;
    [SerializeField] Image bar1;
    [SerializeField] Image bar2;
    public float value1;
    public float value2;
    RectTransform rect;
    public float debug;

    private void Awake()
    {
        rect = transform as RectTransform;
    }
    private void Update()
    {
        debug = handle1.localPosition.x;

    }

    public void OnHandle1ChangeByUser()
    {

    }

    void UpdateValues()
    {
        value2 = Mathf.Clamp01(value2);
        value1 = Mathf.Clamp(value1, 0, value2);

        bar1.fillAmount = value1;
        bar2.fillAmount = value2;
    }
}