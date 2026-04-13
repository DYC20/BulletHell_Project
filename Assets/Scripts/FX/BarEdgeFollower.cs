using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class BarEdgeFollower : MonoBehaviour
{
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectTransform markerRect;

    [SerializeField] private float minNormalized = 0f;
    [SerializeField] private float maxNormalized = 0.8f;

    [SerializeField] private float xOffset = 0f;
    [SerializeField] private float leftPadding = 0f;
    [SerializeField] private float rightPadding = 0f;

    [SerializeField, Range(0f, 1f)] private float previewNormalized = 1f;
    
    public void SetNormalized(float normalized)
    {
        if (barRect == null || markerRect == null)
            return;

        normalized = Mathf.Clamp01(normalized);
        normalized = Mathf.Lerp(minNormalized, maxNormalized, normalized);

        float usableWidth = barRect.rect.width - leftPadding - rightPadding;
        float x = leftPadding + usableWidth * normalized + xOffset;

        Vector2 pos = markerRect.anchoredPosition;
        pos.x = x;
        markerRect.anchoredPosition = pos;
    }
    private void OnValidate()
    {
        SetNormalized(previewNormalized);
    }
}