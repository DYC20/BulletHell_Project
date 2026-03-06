using UnityEngine;

public class WeaponVisualFlip : MonoBehaviour
{
    [SerializeField] private Transform spriteRoot; // the child that holds the SpriteRenderer visuals
    //[SerializeField] private Transform aimReference; // optional (if null, uses this transform)

    private Vector3 _baseScale;

    private void Awake()
    {
        if (spriteRoot == null)
        {
            Debug.LogError($"{name}: WeaponVisualFlip missing spriteRoot reference.");
            enabled = false;
            return;
        }

        _baseScale = spriteRoot.localScale;
        //if (aimReference == null) aimReference = transform;
    }

    // Call this after you aim/rotate the weapon pivot
    public void ApplyFlipFromAimDirection(Vector2 aimDirWorld)
    {
        // if aiming left, flip
        bool faceLeft = aimDirWorld.x < 0f;

        var s = _baseScale;
        s.x = Mathf.Abs(s.x) * (faceLeft ? -1f : 1f);
        spriteRoot.localScale = s;
    }
}
