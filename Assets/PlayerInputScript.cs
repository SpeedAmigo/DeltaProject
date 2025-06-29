using UnityEngine;

public class PlayerInputScript : MonoBehaviour
{
    
    public Camera cam;
    [SerializeField] private Vector2 upwardsModulation;
    [SerializeField] private Vector2 downwardsModulation;
    
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out TerrainModulation terrain))
                {
                    terrain.ModulateTerrain(hit.point, upwardsModulation.x, upwardsModulation.y);
                }
            }
        }
        
        if (Input.GetMouseButton(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out TerrainModulation terrain))
                {
                    terrain.ModulateTerrain(hit.transform.position, downwardsModulation.x, downwardsModulation.y);
                }
            }
        }
    }
}
