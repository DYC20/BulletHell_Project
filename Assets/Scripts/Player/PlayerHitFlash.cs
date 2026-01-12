using UnityEngine;
using System.Collections;

public class PlayerHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashCount = 1;

    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _mpb;
    private Coroutine _flashRoutine;

    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    public void PlayFlash()
    {
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetColor(flashColor);
            yield return new WaitForSeconds(flashDuration);

            SetColor(Color.white);
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private void SetColor(Color color)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(ColorId, color);
        _renderer.SetPropertyBlock(_mpb);
    }
}
