using UnityEngine;

public class CreateFootprint : MonoBehaviour
{
    [SerializeField] GameObject footprintPrefab;
    [SerializeField] GameObject playerFX;
    
    [SerializeField] private Vector3 footprint_01Offset;
    [SerializeField] private Vector3 footprint_02Offset;

    public void CreateLeftFootprint()
    {
        Vector3 FootPos2 = playerFX.transform.position + footprint_02Offset;
        Instantiate(footprintPrefab, FootPos2, Quaternion.identity);
    }
    public void CreateRightFootprint()
    {
        Vector3 FootPos1 = playerFX.transform.position + footprint_01Offset;
        Instantiate(footprintPrefab, FootPos1, Quaternion.identity);
    }
}
