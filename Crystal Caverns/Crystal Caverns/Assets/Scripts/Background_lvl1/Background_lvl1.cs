using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform target; // ��� �����
    [SerializeField] private float parallaxMultiplier = 0.5f;

    private Vector3 previousTargetPosition;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned on ParallaxLayer!");
            return;
        }

        previousTargetPosition = target.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = target.position - previousTargetPosition;

        // �������� ��� Y
        deltaMovement.y = 0f;

        // ��������� ��� ��� ������ ��������� ���� �� X
        transform.position -= deltaMovement * parallaxMultiplier;

        previousTargetPosition = target.position;
    }
}
