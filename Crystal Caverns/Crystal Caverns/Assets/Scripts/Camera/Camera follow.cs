using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    [SerializeField] private float followSpeed = 5f;

    [Header("Camera Offset")]
    [SerializeField] private Vector2 moveCameraOffest = new Vector2(100f, 0f);

    private float halfHeight;
    private float halfWidth;
    private bool isCameraMoved = false;
    private Vector3 targetMovePosition;
    private bool isMovingToTarget = false;

    private void Awake()
    {
        if (!player)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
                player = playerController.transform;
        }

        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    private void Update()
    {
        if (!player) return;

        if (isCameraMoved)
        {
            if (isMovingToTarget)
            {
                transform.position = Vector3.Lerp(transform.position, targetMovePosition, Time.unscaledDeltaTime * followSpeed);

                if (Vector3.Distance(transform.position, targetMovePosition) < 0.1f)
                {
                    transform.position = targetMovePosition;
                    isMovingToTarget = false;
                }
            }
        }
        else
        {
            Vector3 targetPos = player.position;
            targetPos.z = -10f;

            targetPos.x = Mathf.Clamp(targetPos.x, minX + halfWidth, maxX - halfWidth);
            targetPos.y = Mathf.Clamp(targetPos.y, minY + halfHeight, maxY - halfHeight);

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }
    }

    public void MoveCamera()
    {
        isCameraMoved = true;
        isMovingToTarget = true;

        if (player != null)
        {
            Vector3 position = player.position + (Vector3)moveCameraOffest;
            position.z = -10f;

            position.x = Mathf.Clamp(position.x, minX + halfWidth, maxX - halfWidth);
            position.y = Mathf.Clamp(position.y, minY + halfHeight, maxY - halfHeight);

            targetMovePosition = position;

            Debug.Log($"Moving camera to position: {position}");
        }
    }

    public void UnmoveCamera()
    {
        isCameraMoved = false;
        isMovingToTarget = false;
    }
}