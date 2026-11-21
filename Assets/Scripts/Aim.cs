using UnityEngine;

public class Aim : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private GameObject player;

    void Start()
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null) mainCam = camObj.GetComponent<Camera>();
    }
}
