using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;
using System.Threading;

[RequireComponent(typeof(VisualEffect))]
public class AuraManager : MonoBehaviour
{
    [Header("Constants To Tune")]
    [SerializeField] float SafeSeparationZone = 5;
    [SerializeField] float MaxSeparationZone = 10;

    [Header("Aura Pulse Effect Settings")]
    [SerializeField] float InitialScale = 1f;
    [SerializeField] float InitialLifeTime = 1.5f;

    [Header("Colors of the Aura")]
    [SerializeField] Color InitialColor = Color.white;
    [SerializeField] Color MidColor;
    [SerializeField] Color EndColor;

    [Header("Aura Current Status")]
    public bool AuraBroken = false;
    public bool isActive = false;

    private VisualEffect m_AuraEffect;
    private GameObject OtherPlayer;

    private bool inView = false;


    void Start()
    {
        m_AuraEffect = GetComponent<VisualEffect>();
        isActive = false;
        if (GetComponentInParent<SetupDummyAura>() != null)
        {
            return;
        }
        if (GetComponentInParent<NetworkObject>().IsOwner)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        m_AuraEffect.SetFloat("Lifetime", InitialLifeTime);
        m_AuraEffect.SetFloat("Scale", InitialScale);
        m_AuraEffect.SetVector4("Color", InitialColor);
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

    public void PlayAura()
    {
        if (!isActive)
        {
            return;
        }
        if (OtherPlayer == null)
        {
            OtherPlayer = GameManager.LocalPlayerObject;
            return;
        }
        Aura(Vector3.Distance(gameObject.transform.position, OtherPlayer.transform.position));
    }
    private int Aura(float distance)
    {
        //Normalize Distance relative to Min and Max Separation Zones, to Get a value which is Negative While in Safe zone, between safe and max the value will be between 0 and 1
        float NormalizedDistance = Mathf.Min(1, (distance - SafeSeparationZone) / MaxSeparationZone);

        //Return Bool that checks if other player is in view or not
        inView = ObjectInCameraView(transform.parent.gameObject);

        if (!AuraBroken && NormalizedDistance > 0)
        {
            // Value > 0 means the separation distance is > SAFE ZONE
            //Check if you can see the other player
            if (inView)
            {
                //if you can see other player, ripple effect will stop
                m_AuraEffect.Stop();
                return 1;
            }
            //You can't see other player, then we have to play the ripple effect
            else
            {
                // if Value exceeds 1 then the separatino distance > MAX distance and thus the aura breaks!
                if (NormalizedDistance >= 1f)
                {
                    m_AuraEffect.Stop();
                    //AuraBroken = true;
                    return -1;
                }


                // Change the color of the ripples, this will be a gradient from Oranage to Red
                Color ColorOnGrad = Color.Lerp(MidColor, EndColor, NormalizedDistance);
                m_AuraEffect.SetVector4("Color", ColorOnGrad);

                // Scale up based on separation distance, this will ensure that the other player will see ripple effect even at high separation
                m_AuraEffect.SetFloat("Scale", InitialScale + NormalizedDistance*10f);
                m_AuraEffect.SetFloat("Lifetime", InitialLifeTime - NormalizedDistance);
                //Finally Play the effect
                m_AuraEffect.Play();
                return 2;
            }
        }
        //Value < 0 Means the separation is in SAFE ZONE, this means the ripples will be smaller and will have a white/transperant color
        //Moreover if aura was broken and players enter safe zone then the AURA will be re-activated again
        else
        {
            if (NormalizedDistance <= 0)
            {
                AuraBroken = false;
                if (inView)
                {
                    //if you can see other player, ripple effect will stop
                    m_AuraEffect.Stop();
                    return 1;
                }

                m_AuraEffect.SetVector4("Color", InitialColor);
                m_AuraEffect.SetFloat("Scale", InitialScale);
                m_AuraEffect.SetFloat("Lifetime", InitialLifeTime);

                m_AuraEffect.Play();
                return 2;
            }
            return -1;
        }
    }
}
