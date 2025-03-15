using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of WASD movement
    public float jumpSpeed = 30f; // Speed of JumpTo transition
    private Vector3 targetPosition; // Target for JumpTo
    private bool isJumping = false; // Flag for smooth transition
    private Camera cam;
    public SafariMap map;

    public Vector2 minBounds; 
    public Vector2 maxBounds;

    private float minZoom = 1f;
    private float maxZoom = 10f;
    private float zoomSpeed = 5f;

    void Start()
    {
        transform.position = new Vector3(map.map_dimensions.x / 2, map.map_dimensions.y / 2, transform.position.z);
        targetPosition = transform.position;
        cam = GetComponent<Camera>();

        minBounds = new Vector2(-2.2f, 0.9f);
        maxBounds = map.map_dimensions + minBounds;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        SmoothJump();
    }

    void HandleMovement()
    {
        float camHeight = cam.orthographicSize;
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        float speeding = 1f;

        if (moveX != 0 || moveY != 0)
        {
            isJumping= false;
        }

        //If pressing shift double camera speed
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speeding = 2f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            JumpTo(new Vector3(map.map_dimensions.x / 2, map.map_dimensions.y / 2, transform.position.z));
        }

        Vector3 moveDirection = new Vector3(moveX, moveY, 0) * moveSpeed * speeding * Time.deltaTime * (camHeight/5);
        transform.position += moveDirection;

        ClampPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom); // Keep zoom within limits

        ClampPosition();
    }
    void ClampPosition()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x + camWidth, maxBounds.x - camWidth),
            Mathf.Clamp(transform.position.y, minBounds.y + camHeight, maxBounds.y - camHeight),
            transform.position.z
        );
    }

    void SmoothJump()
    {
        if (isJumping)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, jumpSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isJumping = false;
            }
        }
    }

    public void JumpTo(Vector2 coords)
    {
        targetPosition = new Vector3(coords.x, coords.y, transform.position.z);
        isJumping = true;
    }
}
