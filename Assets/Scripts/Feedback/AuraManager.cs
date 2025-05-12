using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(MeshRenderer))]
public class AuraManager : MonoBehaviour
{
    [Header("Constants To Tune")]
    [SerializeField] float SafeSeparationZone = 5;
    [SerializeField] float MaxSeparationZone = 10;
    [SerializeField] float StartingValue = 12;
    [SerializeField] Color StartingColor = new Color(255 / 255f, 252 / 255f, 0 / 255f, 0.5f);
    [SerializeField] Color EndingColor = new Color(255 / 255f, 83 / 255f, 0 / 255f, 0.5f);
    [SerializeField] public float PowerValue = 1f;

    MeshRenderer m_meshRenderer;
    Material m_material;

    [Header("Debug")]
    [SerializeField] GameObject OtherPlayer;
    public bool AuraState = true;
    public bool activateOnLocalPlayer = false;
    public bool AuraBroken = false;

    private bool attachedToLocalPlayer = false;

    [SerializeField] bool inView = false;

    void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
        m_material = m_meshRenderer.sharedMaterial;
        m_meshRenderer.enabled = AuraState;
        if (GetComponentInParent<NetworkObject>() != null)
        {
            if (GetComponentInParent<NetworkObject>().IsOwner)
            {
                attachedToLocalPlayer = true;
            }
        }
    }

    private void OnApplicationQuit()
    {
        gameObject.transform.localScale = new Vector3(10, 10, 10);
        //m_material.SetFloat("_Size", 0.04f);
        m_material.SetColor("_WaveColor", new Color(0.65f, 0.65f, 0.65f, 0.5f));
    }

    public int SetOtherPlayer(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("[AuraManager] Other Player is null");
            return -1;
        }
        OtherPlayer = player;
        return 0;
    }

    private bool ObjectInCameraView(GameObject obj)
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(obj.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        return onScreen;
    }
    private void Update()
    {
        if (!AuraState)
        {
            m_meshRenderer.enabled = false;
            return;
        }
        if (activateOnLocalPlayer)
        {
            if (!attachedToLocalPlayer)
            {
                m_meshRenderer.enabled = false;
                return;
            }
            OtherPlayer = GameManager.RemotePlayerObject;
        }
        else
        {
            OtherPlayer = GameManager.LocalPlayerObject;
        }
        Aura(Vector3.Distance(gameObject.transform.position, OtherPlayer.transform.position));
    }
    public int Aura(float distance)
    {
        //Normalize Distance relative to Min and Max Separation Zones, to Get a value which is Negative While in Safe zone, between safe and max the value will be between 0 and 1
        float value = Mathf.Min(1, (distance - SafeSeparationZone) / MaxSeparationZone);
        Debug.Log("[Aura] Value: " + value);

        //Return Bool that checks if other player is in view or not
        inView = ObjectInCameraView(transform.parent.gameObject);

        if (!AuraBroken && value > 0)
        {
            // Value > 0 means the separation distance is > SAFE ZONE
            //Check if you can see the other player
            if (inView)
            {
                //if you can see other player, ripple effect will stop
                m_meshRenderer.enabled = false;
                return 1;
            }
            //You can't see other player, then we have to play the ripple effect
            else
            {
                // if Value exceeds 1 then the separatino distance > MAX distance and thus the aura breaks!
                if (value >= 1f)
                {
                    m_meshRenderer.enabled = false;
                    AuraBroken = true;
                    return -1;
                }
                // First Scale up based on separation distance, this will ensure that the other player will see ripple effect even at high separation
                //gameObject.transform.localScale = new Vector3(StartingValue + (Mathf.Exp((1+value)* PowerValue) ), StartingValue + (Mathf.Exp((1+value) * PowerValue)), 1f);
                gameObject.transform.localScale = new Vector3(StartingValue + 2 + value * MaxSeparationZone, StartingValue + 2 + value * MaxSeparationZone, 1f);

                //Second is to change the color of the ripples, this will be a gradient from Oranage to Red
                Color ColorOnGrad = Color.Lerp(StartingColor, EndingColor, value);
                ColorOnGrad.a = 0.5f;
                m_material.SetColor("_WaveColor", ColorOnGrad);

                //Finally Play the effect
                m_meshRenderer.enabled = true;
                return 2;
            }
        }
        //Value < 0 Means the separation is in SAFE ZONE, this means the ripples will be smaller and will have a white/transperant color
        //Moreover if aura was broken and players enter safe zone then the AURA will be re-activated again
        else
        {
            if (value <= 0)
            {
                AuraBroken = false;
                if (inView)
                {
                    //if you can see other player, ripple effect will stop
                    m_meshRenderer.enabled = false;
                    return 1;
                }

                gameObject.transform.localScale = new Vector3(StartingValue + 2 + value * MaxSeparationZone, StartingValue + 2 + value * MaxSeparationZone, 1f);
                m_material.SetColor("_WaveColor", new Color(0.65f, 0.65f, 0.65f, 0.5f));
                m_meshRenderer.enabled = true;
                return 2;
            }
            return -1;
        }
    }
}
