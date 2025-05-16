using UnityEngine;

public class ExperimenterManager : MonoBehaviour
{
    [SerializeField] GameObject toggleMenu, statsMenu;
    [SerializeField] GameObject LeftHandRay, RightHandRay;
    private int state = -1;

    private void Start()
    {
        toggleMenu.SetActive(false);
        statsMenu.SetActive(false);
        LeftHandRay.SetActive(false);
        RightHandRay.SetActive(false);
        state = -1;

        //Debug
        //ShowAll();
    }

    private void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.Two)) // && GameManager.Experimenter == GameManager.LocalPlayerObject
        {
            state++;
            if (state>2)
            {
                state = -1;
            }
            UpdateState(state);
        }
    }

    void UpdateState(int state)
    {
        switch (state)
        {
            case 0:
                statsMenu.gameObject.transform.forward = Camera.main.transform.forward.normalized;
                statsMenu.gameObject.transform.position = GameManager.LocalPlayerObject.transform.position + Camera.main.transform.forward.normalized * 2 + new Vector3(0, 0.8f, 0);
                Vector3 InfrontOfPlayer = Vector3.Cross(Camera.main.transform.forward.normalized, Camera.main.transform.up.normalized).normalized;
                toggleMenu.gameObject.transform.forward = InfrontOfPlayer;
                toggleMenu.gameObject.transform.position = GameManager.LocalPlayerObject.gameObject.transform.position + InfrontOfPlayer * 1.25f + new Vector3(0, 0.8f, 0);
                //Show all
                LeftHandRay.SetActive(true);
                RightHandRay.SetActive(true);
                toggleMenu.SetActive(true);
                statsMenu.SetActive(true);
                break;
            case 1:
                LeftHandRay.SetActive(false);
                RightHandRay.SetActive(false);
                toggleMenu.SetActive(false);
                break;
            case 2:
                statsMenu.SetActive(false);
                break;
            case -1:
                break;
        }
    }

    [ContextMenu("Show Menu")]
    public void ShowAll()
    {
        state++;
        if (state > 2)
        {
            state = -1;
        }
        UpdateState(state);
    }
}
