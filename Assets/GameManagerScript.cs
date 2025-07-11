using Unity.Cinemachine;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance;
    
    [SerializeField] private GameObject cameraPrefab;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject CreatePlayerCamera(Transform playerTransform)
    {
        GameObject camObj = Instantiate(cameraPrefab);
        var cam = camObj.GetComponent<CinemachineCamera>();

        if (cam != null)
        {
            cam.Follow = playerTransform;
            cam.LookAt = playerTransform;
        }

        return camObj;
    }
}
