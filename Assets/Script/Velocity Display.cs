using UnityEngine;
using UnityEngine.UI;

public class VelocityDisplay : MonoBehaviour
{
    public Rigidbody2D rb;
    public Text velocityText;

    void Start()
    {
        // Assuming the GameObject has a Rigidbody2D attached
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Create a UI Text object for displaying the velocity
        CreateVelocityText();
    }

    void Update()
    {
        // Display the velocity in the UI Text
        UpdateVelocityText();
    }

    void CreateVelocityText()
    {
        // Create a UI Text object as a child of the canvas
        GameObject canvas = new GameObject("Canvas");
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject textObject = new GameObject("VelocityText");
        textObject.transform.SetParent(canvas.transform);
        velocityText = textObject.AddComponent<Text>();
        velocityText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        velocityText.fontSize = 16;
        velocityText.color = Color.white;
    }

    void UpdateVelocityText()
    {
        // Update the displayed velocity
        if (rb != null && velocityText != null)
        {
            float velocityX = rb.velocity.x;
            float velocityY = rb.velocity.y;
            velocityText.text = $"X: {velocityX:F2} \nY: {velocityY:F2}";
        }
    }
}
