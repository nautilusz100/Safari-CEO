using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Model.Map;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages camera movement, zooming, and jump-to-position functionality in the map.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public float MoveSpeed { get; set; } = 10f; // Speed of WASD movement
    public float JumpSpeed { get; set; } = 30f; // Speed of JumpTo transition
    private Vector3 targetPosition; // Target for JumpTo
    private bool isJumping = false; // Flag for smooth transition
    private Camera cam;
    public SafariMap Map;

    public Vector2 MinBounds { get; set; }
    public Vector2 MaxBounds { get; set; }

    private float minZoom = 1f;
    private float maxZoom = 20f;
    private float zoomSpeed = 5f;

    void Start()
    {
        // Set initial camera position to center of the map
        transform.position = new Vector3(Map.map_dimensions.x / 2, Map.map_dimensions.y / 2, transform.position.z);
        targetPosition = transform.position;
        cam = GetComponent<Camera>();

        // Define map bounds
        MinBounds = new Vector2(-2.2f, 0.9f);
        MaxBounds = Map.map_dimensions + MinBounds;
    }

    void Update()
    {

        HandleMovement();
        HandleZoom();
        SmoothJump();
    }

    /// <summary>
    /// Handles camera movement with WASD or arrow keys.
    /// Holding Shift increases speed.
    /// Pressing Space jumps to map center.
    /// </summary>
    void HandleMovement()
    {
        float camHeight = cam.orthographicSize;
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        float speeding = 1f;

        // Cancel jump movement when manual input occurs
        if (moveX != 0 || moveY != 0)
        {
            isJumping = false;
        }


        // Deselect any selected UI element if it's not a text input
        if ((moveX != 0 || moveY != 0) &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null &&
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() == null)
        {
            // Only deselect if moving and the selected object is not a TMP_InputField
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        // Increase speed when holding Shift
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speeding = 2f;
        }


        // Jump to map center when pressing Space
        if (Input.GetKey(KeyCode.Space))
        {
            JumpTo(new Vector3(Map.map_dimensions.x / 2, Map.map_dimensions.y / 2, transform.position.z));
        }

        // Calculate movement based on camera zoom and apply it
        Vector3 moveDirection = new Vector3(moveX, moveY, 0) * MoveSpeed * speeding * Time.deltaTime * (camHeight/5);
        transform.position += moveDirection;

        ClampPosition(); // Prevent camera from leaving the map
    }

    /// <summary>
    /// Handles zooming in and out using the mouse scroll wheel.
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); // Ensure camera remains within bounds after zoom

        ClampPosition(); // Ensure camera remains within bounds after zoom
    }
    /// <summary>
    /// Prevents the camera from moving outside defined map bounds.
    /// </summary>
    void ClampPosition()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, MinBounds.x + camWidth, MaxBounds.x - camWidth),
            Mathf.Clamp(transform.position.y, MinBounds.y + camHeight, MaxBounds.y - camHeight),
            transform.position.z
        );
    }

    /// <summary>
    /// Smoothly interpolates the camera's position to the target position if jumping.
    /// </summary>
    void SmoothJump()
    {
        if (isJumping)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, JumpSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isJumping = false;
            }
        }
    }
    /// <summary>
    /// Starts a smooth jump to the given map coordinates.
    /// </summary>
    public void JumpTo(Vector2 coords)
    {
        targetPosition = new Vector3(coords.x, coords.y, transform.position.z);
        isJumping = true;
    }
}
