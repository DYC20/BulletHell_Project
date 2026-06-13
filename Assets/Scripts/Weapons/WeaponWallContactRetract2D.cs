using UnityEngine;

public class WeaponWallContactRetract2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponRootToMove;
    [SerializeField] private Collider2D weaponCollider;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallMask;

    [Header("Retract")]
    [SerializeField] private float retractDistance = 0.25f;
    [SerializeField] private float retractSpeed = 18f;
    [SerializeField] private float returnSpeed = 14f;

    [Tooltip("Usually true for your setup if the weapon points along local UP.")]
    [SerializeField] private bool retractOppositeWeaponUp = true;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = true;

    [Header("State")]
    [SerializeField] private bool touchingWall;
    public bool TouchingWall => touchingWall;

    private ContactFilter2D wallFilter;
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];

    private Vector3 defaultLocalPosition;
    private Vector3 currentLocalOffset;

    private void Awake()
    {
        if (weaponRootToMove == null)
            weaponRootToMove = transform;

        if (weaponCollider == null)
            weaponCollider = GetComponentInChildren<Collider2D>();

        defaultLocalPosition = weaponRootToMove.localPosition;

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallMask);
        wallFilter.useLayerMask = true;
        wallFilter.useTriggers = false;
    }

    private void LateUpdate()
    {
        if (weaponRootToMove == null || weaponCollider == null)
            return;

        Physics2D.SyncTransforms();

        int contactCount = weaponCollider.GetContacts(wallFilter, contacts);

        touchingWall = contactCount > 0;

        Vector3 targetOffset = Vector3.zero;

        if (touchingWall)
        {
            targetOffset = GetRetractOffset(contactCount);
        }

        float speed = touchingWall ? retractSpeed : returnSpeed;

        currentLocalOffset = Vector3.MoveTowards(
            currentLocalOffset,
            targetOffset,
            speed * Time.deltaTime
        );

        weaponRootToMove.localPosition = defaultLocalPosition + currentLocalOffset;

        if (drawDebug)
        {
            Color color = touchingWall ? Color.red : Color.green;
            Debug.DrawRay(weaponRootToMove.position, weaponRootToMove.up * 0.4f, color, 0f);
        }
    }

    private Vector3 GetRetractOffset(int contactCount)
    {
        if (retractOppositeWeaponUp)
        {
            Vector3 worldRetractDirection = -weaponRootToMove.right;
            Vector3 localRetractDirection = WorldDirectionToLocalDirection(worldRetractDirection);

            return localRetractDirection.normalized * retractDistance;
        }

        Vector2 averageNormal = Vector2.zero;

        for (int i = 0; i < contactCount; i++)
        {
            averageNormal += contacts[i].normal;

            if (drawDebug)
            {
                Debug.DrawRay(
                    contacts[i].point,
                    contacts[i].normal * 0.25f,
                    Color.yellow,
                    0f
                );
            }
        }

        if (averageNormal.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 localNormal = WorldDirectionToLocalDirection(averageNormal.normalized);
        return localNormal.normalized * retractDistance;
    }

    private Vector3 WorldDirectionToLocalDirection(Vector3 worldDirection)
    {
        if (weaponRootToMove.parent == null)
            return worldDirection;

        return weaponRootToMove.parent.InverseTransformDirection(worldDirection);
    }
}