using Unity.Cinemachine;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera mainMenuCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenuCamera.gameObject.SetActive(true);
    }

    public void OnStartClicked()
    {
        mainMenuCamera.gameObject.SetActive(false);
    }
}
