using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidingArrowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject Arrow;

    [Header("Constants To Tune")]
    [SerializeField] float SafeSeparationZone = 5;
    [SerializeField] float MaxSeparationZone = 10;

    [Header("Colors of the Light")]
    [SerializeField] Color InitialColor = Color.white;
    [SerializeField] Color MidColor;
    [SerializeField] Color EndColor;

    [Header("Blinking Properties")]
    [SerializeField] float MinimumBlinkingSpeed = 4f;

    private Camera vrCamera;
    private GameObject OtherPlayer;
    private Material BlinkingMaterial;
    private float customTime;

    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        Arrow.SetActive(false);
        BlinkingMaterial = Arrow.GetComponent<MeshRenderer>().sharedMaterial;
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

        if (!ObjectInCameraView(OtherPlayer) ) //The Object is not in the FOV of the Camera
        {
            float Distance = Vector3.Distance(vrCamera.transform.position, OtherPlayer.transform.position);
            if (Distance > MaxSeparationZone)
            {
                Arrow.SetActive(false);
                return;
            }
            Arrow.transform.LookAt(OtherPlayer.transform.position); //Make the Arrow look at the Camera
            ComputerColorFrequency(Distance);
            Arrow.SetActive(true);
        }
        else //Object is in the FOV of the Camera
        {
            Arrow.SetActive(false);
        }
    }

    private void ComputerColorFrequency(float Distance)
    {
        float value = Mathf.Min(1, (Distance - SafeSeparationZone) / MaxSeparationZone);
        Color color = InitialColor;
        float frequency = MinimumBlinkingSpeed;
        if (value > 0)
        {
            color = Color.Lerp(MidColor, EndColor, value);
            frequency = Mathf.Lerp(frequency, frequency*2, value);
        }
        BlinkingMaterial.SetColor("_Color", color);
        BlinkingMaterial.SetFloat("_Speed", frequency);
    }
    public void SetOtherPlayer(GameObject player)
    {
        OtherPlayer = player;
    }
}
