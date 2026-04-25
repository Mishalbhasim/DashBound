// Scripts/Camera/ParallaxBackground.cs  — replace your existing one
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0.1f;

    private Camera cam;
    private Vector3 lastCamPos;
    private float textureUnitSizeX;  // width of sprite in world units

    private void Start()
    {
        cam = Camera.main;
        lastCamPos = cam.transform.position;

        // Stick to camera immediately on start — no black edges ever
        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z
        );

        // Get the world width of the sprite for infinite tiling
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        if (sprite != null)
            textureUnitSizeX = sprite.texture.width / sprite.pixelsPerUnit;
    }

    private void LateUpdate()
    {
        // How much the camera moved this frame
        Vector3 delta = cam.transform.position - lastCamPos;

        // Move background by parallax fraction of camera movement
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor * 0.3f, 0);

        // Infinite scroll trick — when camera moves far enough,
        // jump the background by exactly one texture width so it tiles seamlessly
        float distFromCamX = cam.transform.position.x - transform.position.x;
        if (Mathf.Abs(distFromCamX) >= textureUnitSizeX)
        {
            transform.position += new Vector3(
                Mathf.Sign(distFromCamX) * textureUnitSizeX,
                0, 0
            );
        }

        lastCamPos = cam.transform.position;
    }
}