using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidingArrowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera vrCamera;
    [SerializeField] GameObject Arrow;
    [Header("Users to Track")]
    [SerializeField] GameObject OtherPlayer;

    [SerializeField] Color YellowColor = new Color(1f, 1f, 1f);
    [SerializeField] Color RedColor = new Color(1f, 0f, 0f);
    private Material BlinkingMaterial;

    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        Arrow.SetActive(false);
        BlinkingMaterial = Arrow.GetComponent<MeshRenderer>().sharedMaterial;
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
        if (OtherPlayer == null)
        {
            return;
        }

        if (!ObjectInCameraView(OtherPlayer) ) //The Object is not in the FOV of the Camera
        {
            Arrow.transform.LookAt(OtherPlayer.transform.position); //Make the Arrow look at the Camera
            ComputerColorFrequency();
            Arrow.SetActive(true);
        }
        else //Object is in the FOV of the Camera
        {
            Arrow.SetActive(false);
        }
    }

    private void ComputerColorFrequency()
    {
        float Distance = Vector3.Distance(vrCamera.transform.position, OtherPlayer.transform.position);
        float value = Mathf.Min(1, (Distance - 5) / 10);
        Color color = new Color(1f, 1f, 1f);
        float period = 2f;
        if (value > 0)
        {
            color = Color.Lerp(YellowColor, RedColor, value);
            period = Mathf.Lerp(2f, 5f, value);
        }
        BlinkingMaterial.SetColor("_Color", color);
        BlinkingMaterial.SetFloat("_Speed", period);
    }
    public void SetOtherPlayer(GameObject player)
    {
        OtherPlayer = player;
    }
}
