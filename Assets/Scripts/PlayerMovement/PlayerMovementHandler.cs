using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Avatar2;
using UnityEngine;

[RequireComponent(typeof(Armswing))]
//[RequireComponent(typeof(CharacterController))]
public class PlayerMovementHandler : MonoBehaviour
{
    PlayerMovementData m_data;
    MecanimLegsAnimationController mecanimLegsAnimationController;
    Armswing m_armSwing;
    CharacterController m_characterController;

    public float MAXVALUE = 0.02f;
    public float MINVALUE = -0.02f;


    public bool DEBUG = false;

    // Start is called before the first frame update
    void Start()
    {
        m_armSwing = GetComponent<Armswing>();
        m_characterController = GetComponent<CharacterController>();
        m_data = new PlayerMovementData(transform.position, Vector3.zero, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (mecanimLegsAnimationController == null)
        {
            mecanimLegsAnimationController = GameManager.LocalPlayerObject.GetComponentInChildren<MecanimLegsAnimationController>();
            return;
        }
        SetMovementData(m_armSwing.GetSpeedFromSwings());

        if (CheckPlayerInput())
        {
            //if the player has movement enabled
            moveplayer();
        }
        else
        {
            mecanimLegsAnimationController.armswing = Vector3.zero;
        }
        UpdateNetworkInfo();
    }

    private void UpdateNetworkInfo()
    {
        if (DEBUG)
        {
            GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>().UpdateValues(
            m_data.Direction,
            10f,
            3f
            );
            return;
        }

        GameManager.LocalPlayerObject?.GetComponent<PlayerNetworkInfo>().UpdateValues(
            m_data.Direction,
            m_data.Speed,
            m_data.CycleDuration
            );
    }


    bool CheckPlayerInput()
    {
        return
        (OVRInput.Get(OVRInput.Touch.PrimaryThumbRest) || OVRInput.Get(OVRInput.Touch.SecondaryThumbRest)
                || OVRInput.Get(OVRInput.Touch.One) || OVRInput.Get(OVRInput.Touch.Two)
                || OVRInput.Get(OVRInput.Touch.Three) || OVRInput.Get(OVRInput.Touch.Four));
    }

    private void moveplayer()
    {
        Vector3 value = m_data.Speed * m_data.Direction * Time.deltaTime;
        mecanimLegsAnimationController.armswing = new Vector3( Mathf.Clamp(value.x, MINVALUE, MAXVALUE) , 0, Mathf.Clamp(value.z, MINVALUE, MAXVALUE));
        m_characterController.SimpleMove(value);
    }



    public void SetMovementData(PlayerMovementData data)
    {
        m_data = data;
    }

    public PlayerMovementData GetMovementData()
    {
        return m_data;
    }

}
