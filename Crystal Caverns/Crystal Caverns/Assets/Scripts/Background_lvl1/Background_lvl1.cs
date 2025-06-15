using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform target; // наш герой
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

        // Ігноруємо вісь Y
        deltaMovement.y = 0f;

        // Інвертуємо рух для ефекту паралаксу лише по X
        transform.position -= deltaMovement * parallaxMultiplier;

        previousTargetPosition = target.position;
    }
}
