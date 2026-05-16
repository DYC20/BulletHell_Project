using UnityEngine;

public class FitRenderTextureQuadToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distanceFromCamera = 5f;

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;

        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        transform.rotation = targetCamera.transform.rotation;
        transform.localScale = new Vector3(width, height, 1f);
    }
}