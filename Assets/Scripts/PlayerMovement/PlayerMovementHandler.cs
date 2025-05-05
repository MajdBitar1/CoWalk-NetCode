using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Armswing))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovementHandler : MonoBehaviour
{
    PlayerMovementData m_data;

    Armswing m_armSwing;
    CharacterController m_characterController;

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

        if (CheckPlayerInput())
        {
            //if the player has movement enabled
            SetMovementData(m_armSwing.GetSpeedFromSwings());
            moveplayer();
        }
        else
        {
            //m_armSwing.InitializeSwinger();
        }
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
