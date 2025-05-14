using System.Collections;
using System.Collections.Generic;
using Oculus.Platform;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ColorInGrayOut : MonoBehaviour
{
    [SerializeField] GameObject OtherPlayer;
    private Camera m_camera;
    private MeshRenderer m_meshRenderer;
    private Material m_sharedMaterial;

    [Header("Tuning Values")]
    public float SafeSeparationZone = 5f;
    public float MaxSeparationZone = 10f;

    // Start is called before the first frame update
    void Start()
    {
        m_camera = Camera.main;
        m_meshRenderer = GetComponent<MeshRenderer>();
        if (m_meshRenderer != null)
        {
            m_sharedMaterial = m_meshRenderer.sharedMaterial;
        }
        else
        {
            Debug.LogError("MeshRenderer not found on the GameObject.");
        }
    }

    private void OnEnable()
    {
        if (m_sharedMaterial != null)
        {
            m_sharedMaterial.SetVector("_Center", OtherPlayer.transform.position);
            m_sharedMaterial.SetFloat("_BlendValue", 0f);
            m_sharedMaterial.EnableKeyword("_ENABLED");
        }
    }
    private void OnDisable()
    {
        m_sharedMaterial.SetFloat("_BlendValue", 0f);
        m_sharedMaterial.DisableKeyword("_ENABLED");
    }

    private void LateUpdate()
    {
        if (m_camera == null || m_sharedMaterial == null || OtherPlayer == null)
        {
            m_sharedMaterial.DisableKeyword("_ENABLED");
            return;
        }
        m_sharedMaterial.SetVector("_Center", OtherPlayer.transform.position);
        float Distance = Vector3.Distance(m_camera.transform.position, OtherPlayer.transform.position);
        float value = Mathf.Min(1, (Distance - SafeSeparationZone) / MaxSeparationZone);

        if (value < 0)
        {
            m_sharedMaterial.SetFloat("_BlendValue", 0);
        }
        else
        {
            
            m_sharedMaterial.SetFloat("_BlendValue", value);
        }
    }

    private void OnApplicationQuit()
    {
        if (m_sharedMaterial != null)
        {
            m_sharedMaterial.SetVector("_Center", new Vector3(0, 0, 0));
            m_sharedMaterial.SetFloat("_BlendValue", 0f);
            m_sharedMaterial.DisableKeyword("_ENABLED");
        }
    }

    public void SetOtherPlayer(GameObject player)
    {
        OtherPlayer = player;
    }
}
