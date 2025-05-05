using UnityEngine;

public class ExperimenterManager : MonoBehaviour
{
    [SerializeField] GameObject toggleMenu, statsMenu;

    [SerializeField] GameObject LeftHandRay, RightHandRay;

    private bool _ShowMenu = false;

    private void Start()
    {
        toggleMenu.SetActive(false);
        statsMenu.SetActive(false);
        LeftHandRay.SetActive(false);
        RightHandRay.SetActive(false);
    }

    public void ConnectToExperimenter()
    {

    }

    private void Update()
    {
        if (OVRInput.GetUp(OVRInput.Button.Two) && GameManager.Experimenter == GameManager.LocalPlayerObject)
        {
            ShowMenu();
        }
    }

    [ContextMenu("Show Menu")]
    public void ShowMenu()
    {
        _ShowMenu = !_ShowMenu;
        if (_ShowMenu)
        {
            LeftHandRay.SetActive(true);
            RightHandRay.SetActive(true);

            //Vector3 InfrontOfPlayer = Vector3.Cross(Camera.main.transform.forward.normalized, Camera.main.transform.up.normalized).normalized;

            //statsMenu.gameObject.transform.forward = Camera.main.transform.forward.normalized;
            //statsMenu.gameObject.transform.position = GameManager.Experimenter.gameObject.transform.position + Camera.main.transform.forward.normalized * 3  + new Vector3(0, 1f, 0);
            statsMenu.SetActive(true);
            
            //toggleMenu.gameObject.transform.forward = InfrontOfPlayer;
            //toggleMenu.gameObject.transform.position = GameManager.Experimenter.gameObject.transform.position + InfrontOfPlayer * 3 + new Vector3(0, 1f, 0);
            toggleMenu.SetActive(true);
        }
        else
        {
            statsMenu.SetActive(false);
            toggleMenu.SetActive(false);
            LeftHandRay.SetActive(false);
            RightHandRay.SetActive(false);
        }
    }
}
