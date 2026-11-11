using UnityEngine;

public class ParallaxInfinite : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Range(0f, 1f)]
    [SerializeField] private float parallaxFactor = 0.2f;

    [SerializeField] private float planeWidth = 20f;

    private float startX;

    private void Start()
    {
        startX = transform.position.x;
    }

    private void LateUpdate()
    {
        float movement = cameraTransform.position.x * parallaxFactor;

        transform.position = new Vector3(
            startX + movement,
            transform.position.y,
            transform.position.z
        );

        float distanceToCamera = cameraTransform.position.x - transform.position.x;

        if (distanceToCamera > planeWidth)
        {
            startX += planeWidth * 2f;
        }
        else if (distanceToCamera < -planeWidth)
        {
            startX -= planeWidth * 2f;
        }
    }
}