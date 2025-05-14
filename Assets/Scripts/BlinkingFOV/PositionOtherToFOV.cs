using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.UnityCanvas;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        indicator.SetActive(false);
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

    public void SetOtherPlayer(GameObject player)
    {
        OtherPlayer = player;
    }
}
