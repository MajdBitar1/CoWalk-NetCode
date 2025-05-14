using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.UnityCanvas;
using UnityEngine;
using UnityEngine.UI;

public class PositionOtherToFOV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera vrCamera;
    [SerializeField] RectTransform canvasRect;
    [SerializeField] GameObject indicator;

    [Header("Users to Track")]
    [SerializeField] GameObject OtherPlayer;

    [Header("Indicator Settings")]
    [SerializeField] float HorizontalOffset = 0.3f;
    [SerializeField] float VerticalOffset = -0.1f;

    [SerializeField] Color YellowColor = new Color(1f, 1f, 1f);
    [SerializeField] Color RedColor = new Color(1f, 0f, 0f);
    private Material BlinkingMaterial;


    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        indicator.SetActive(false);
        BlinkingMaterial = indicator.GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (OtherPlayer == null)
        {
            return;
        }

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(OtherPlayer.transform.position);

        if (viewportPos.z < 0) //The Object is not in the FOV of the Camera
        {
            //Assume the object is to the Left of the Camera
            float ValueX = 0f + HorizontalOffset;

            Vector3 toObject = OtherPlayer.transform.position - vrCamera.transform.position;
            if (Vector3.Dot(toObject, vrCamera.transform.right) > 0)
            {
                //Object is to the Right of the Camera
                ValueX = 1f - HorizontalOffset;
            }
            ComputerColorFrequency();
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
