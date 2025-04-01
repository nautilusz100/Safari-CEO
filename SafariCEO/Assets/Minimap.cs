using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Camera mainCamera; // A fõ kamera
    public RectTransform minimapFrame; // A fehér téglalap (UI)
    public Camera minimapCamera; // A minimap kamerája
    public SafariMap safariMap;
    public RectTransform minimapUI;
    float mapSize;

    private void Start()
    {
        mapSize = safariMap.map_dimensions.x;
        if (minimapCamera == null) return;

        // Get the screen aspect ratio
        float aspectRatio = (float)Screen.width / Screen.height;

        // Adjust the orthographic size to fit the entire map
        minimapCamera.orthographicSize = mapSize / 2f; // Half of the map height

        // If the aspect ratio is wider than 1:1, adjust for width
        /*if (aspectRatio > 1f)
        {
            minimapCamera.orthographicSize = (mapSize / 2f) / aspectRatio;
        }*/

        // Set the camera position to the center of the map
        minimapCamera.transform.position = new Vector3(mapSize / 2f - 2.2f, mapSize / 2f + 0.9f, -10f);
    }

    private void Update()
    {

        if (mainCamera == null || minimapUI == null || minimapFrame == null) return;

        // Get the minimap UI size in pixels
        float minimapWidth = minimapUI.rect.width;
        float minimapHeight = minimapUI.rect.height;

        // Get the world position of the main camera
        Vector3 camPos = mainCamera.transform.position;

        // Normalize the camera position (0 to 1 range)
        float normX = camPos.x / mapSize;
        float normY = camPos.y / mapSize;

        // Convert to minimap position
        float minimapPosX = (normX * minimapWidth) - (minimapWidth / 2) +9f;
        float minimapPosY = (normY * minimapHeight) - (minimapHeight / 2) -5f;

        // Update the rectangle position
        minimapFrame.anchoredPosition = new Vector2(minimapPosX, minimapPosY);

        // Scale the rectangle based on the main camera view
        float camSize = mainCamera.orthographicSize * 2f;
        float camWidth = camSize * mainCamera.aspect;
        float camHeight = camSize;

        minimapFrame.sizeDelta = new Vector2(
            (camWidth / mapSize) * minimapWidth,
            (camHeight / mapSize) * minimapHeight
        );
    }
}
