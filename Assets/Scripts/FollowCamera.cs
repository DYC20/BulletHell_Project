using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Vector3 offset;
    
    private Transform target;
    private Vector3 originalPosition;
    private Vector3 newPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cam == null)
            cam = Camera.main;
        target = cam.transform;
        originalPosition = this.transform.position;
        newPosition = originalPosition;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = Vector3.Lerp(newPosition, target.position, Time.deltaTime) + offset;
        newPosition = this.transform.position;
    }
}
