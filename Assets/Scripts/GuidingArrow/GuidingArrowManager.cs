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


    // Start is called before the first frame update
    void Start()
    {
        vrCamera = Camera.main;
        Arrow.SetActive(false);
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
            Arrow.transform.LookAt(OtherPlayer.transform.position); //Make the Arrow look at the Camera
            Arrow.SetActive(true);
        }
        else //Object is in the FOV of the Camera
        {
            Arrow.SetActive(false);
        }
    }
}
