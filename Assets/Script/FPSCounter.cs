using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Reference to the TextMeshProUGUI component
    private float deltaTime = 0.0f;
    public float updateInterval = 1.0f; // Update the FPS display every 1 second
    private float timeSinceLastUpdate = 0.0f;

    void Update()
    {
        // Calculate the delta time between frames
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Accumulate time since the last update
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        // Update the FPS display every `updateInterval` seconds
        if (timeSinceLastUpdate >= updateInterval)
        {
            // Calculate the FPS
            float fps = 1.0f / deltaTime;

            // Update the FPS text
            fpsText.text = string.Format("{0:0.} FPS", fps);

            // Reset the timer
            timeSinceLastUpdate = 0.0f;
        }
    }
}