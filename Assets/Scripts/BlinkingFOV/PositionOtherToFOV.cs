using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.UnityCanvas;
using UnityEngine;
using UnityEngine.UI;

public class PositionOtherToFOV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform canvasRect;
    [SerializeField] GameObject indicator;

    [Header("Constants To Tune")]
    [SerializeField] float SafeSeparationZone = 5;
    [SerializeField] float MaxSeparationZone = 10;

    [Header("Colors of the Light")]
    [SerializeField] Color InitialColor = Color.white;
    [SerializeField] Color MidColor;
    [SerializeField] Color EndColor;

    [Header("Blinking Properties")]
    [SerializeField] float MinimumBlinkingSpeed = 4f;

    [Header("Indicator Settings")]
    [SerializeField] float HorizontalOffset = 0.3f;
    [SerializeField] float VerticalOffset = -0.1f;

    private Camera vrCamera;
    private GameObject OtherPlayer;
    private Material BlinkingMaterial;
    private float customTime;


    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        indicator.SetActive(false);
        BlinkingMaterial = indicator.GetComponent<Image>().material;
        BlinkingMaterial.SetColor("_Color", InitialColor);
    }
    private void OnApplicationQuit()
    {
        BlinkingMaterial.SetColor("_Color", InitialColor);
        BlinkingMaterial.SetFloat("_Speed", MinimumBlinkingSpeed);
        BlinkingMaterial.SetFloat("_CustomTime", 0f);
    }
    private bool ObjectInCameraView(GameObject obj)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(obj.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        return onScreen;
    }
    // Update is called once per frame
    void Update()
    {
        customTime += Time.unscaledDeltaTime;
        BlinkingMaterial.SetFloat("_CustomTime", customTime);

        if (OtherPlayer == null)
        {
            return;
        }

        if (!ObjectInCameraView(OtherPlayer)) //The Object is not in the FOV of the Camera
        {
            //Assume the object is to the Left of the Camera
            float ValueX = 0f + HorizontalOffset;

            Vector3 toObject = OtherPlayer.transform.position - vrCamera.transform.position;
            if (Vector3.Dot(toObject, vrCamera.transform.right) > 0)
            {
                //Object is to the Right of the Camera
                ValueX = 1f - HorizontalOffset;
            }

            Vector2 anchoredPos = new Vector2(
                (ValueX - 0.5f) * canvasRect.rect.width,
                (VerticalOffset) * canvasRect.rect.height
            );
            indicator.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
            indicator.SetActive(true);
        }
        else //Object is in the FOV of the Camera
        {
            indicator.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        ComputerColorFrequency();
    }
    private void ComputerColorFrequency()
    {
        float Distance = Vector3.Distance(vrCamera.transform.position, OtherPlayer.transform.position);
        float value = Mathf.Min(1, (Distance - SafeSeparationZone) / MaxSeparationZone);
        Color color = InitialColor;
        float frequency = MinimumBlinkingSpeed;
        if (value > 0)
        {
            color = Color.Lerp(MidColor, EndColor, value);
            frequency = Mathf.Lerp(frequency, frequency*2, value);
        }
        BlinkingMaterial.SetColor("_Color", color);
        BlinkingMaterial.SetFloat("_Speed", frequency*2);
    }
    public void SetOtherPlayer(GameObject player)
    {
        OtherPlayer = player;
    }
}
