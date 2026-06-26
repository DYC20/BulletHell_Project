using UnityEngine;

public class ParallexLayer : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Range(0f, 2f)]
    [SerializeField] private float parallaxFactor = 0.5f;

    [SerializeField] private float referenceSize;
    [SerializeField] private bool onlyXAxis = true;
    
    
    private Vector3 lastCameraPosition;

    private void Awake()
    {
     
    }

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 cameraDelta = cameraTransform.position - lastCameraPosition;
  
            float scale = transform.localScale.x / referenceSize;

            if (onlyXAxis)
            {
                transform.position += new Vector3(
                    cameraDelta.x * parallaxFactor,
                    0f,
                    0f
                ) * scale;
            }
            else
            {
                transform.position += new Vector3(
                    cameraDelta.x * parallaxFactor,
                    cameraDelta.y * parallaxFactor,
                    0f
                ) * scale;
            }
        lastCameraPosition = cameraTransform.position;
    }


}
