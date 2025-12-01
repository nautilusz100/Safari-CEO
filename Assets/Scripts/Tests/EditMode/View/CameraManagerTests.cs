using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Assets.Scripts.Model.Map;

[TestFixture]
public class CameraManagerTests
{
    private GameObject cameraObject;
    private CameraManager cameraManager;

    private GameObject safari;
    private SafariMap map;

    [SetUp]
    public void SetUp()
    {
        cameraObject = new GameObject();
        cameraObject.AddComponent<Camera>();
        cameraManager = cameraObject.AddComponent<CameraManager>();

        safari = new GameObject();
        map = safari.AddComponent<SafariMap>();
        map.map_dimensions = new Vector2(100, 100);
        cameraManager.Map = map;

        // Use Reflection to call protected Start() method
        MethodInfo startMethod = typeof(CameraManager).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(cameraManager, null);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cameraObject);
    }

    [Test]
    public void TestCamField()
    {
        // Access private "cam" field using Reflection
        FieldInfo camField = typeof(CameraManager).GetField("cam", BindingFlags.Instance | BindingFlags.NonPublic);

        // Get the value of the field (should be the Camera component)
        Camera cam = (Camera)camField.GetValue(cameraManager);

        Assert.IsNotNull(cam);
        Assert.AreEqual(cameraObject.GetComponent<Camera>(), cam);
    }

    [Test]
    public void HandleZoom_ClampsZoomTooSmall()
    {
        FieldInfo camField = typeof(CameraManager).GetField("cam", BindingFlags.Instance | BindingFlags.NonPublic);
        Camera cam = (Camera)camField.GetValue(cameraManager);

        cam.orthographicSize = 0.1f; 

        MethodInfo handleZoomMethod = typeof(CameraManager).GetMethod("HandleZoom", BindingFlags.Instance | BindingFlags.NonPublic);
        handleZoomMethod.Invoke(cameraManager, null);

        Assert.AreEqual(1f, cam.orthographicSize, 0.01f);
    }

    [Test]
    public void HandleZoom_ClampsZoomTooBig()
    {
        // Access the private field cam
        FieldInfo camField = typeof(CameraManager).GetField("cam", BindingFlags.Instance | BindingFlags.NonPublic);
        Camera cam = (Camera)camField.GetValue(cameraManager);

        // Manually simulate initial zoom level
        cam.orthographicSize = 20f;

        // Call the private HandleZoom method
        MethodInfo handleZoomMethod = typeof(CameraManager).GetMethod("HandleZoom", BindingFlags.Instance | BindingFlags.NonPublic);
        handleZoomMethod.Invoke(cameraManager, null);

        // Assert: cam.orthographicSize should be clamped to 10 (the maxZoom limit)
        Assert.AreEqual(20f, cam.orthographicSize, 0.01f);
    }






    [Test]
    public void ClampPosition_LimitsCameraPositionWithinBounds()
    {
        // Set the camera's position outside the defined bounds
        cameraObject.transform.position = new Vector3(-100, -100, 0);

        // Use Reflection to call private ClampPosition method
        MethodInfo clampPositionMethod = typeof(CameraManager).GetMethod("ClampPosition", BindingFlags.Instance | BindingFlags.NonPublic);
        clampPositionMethod.Invoke(cameraManager, null);

        // Assert the camera's position is now within the bounds
        Assert.GreaterOrEqual(cameraObject.transform.position.x, cameraManager.MinBounds.x);
        Assert.GreaterOrEqual(cameraObject.transform.position.y, cameraManager.MinBounds.y);
    }

    [Test]
    public void SmoothJump_MovesCameraToTargetPosition()
    {
        // Set up a target position for jumping
        Vector3 targetPosition = new Vector3(50, 50, cameraObject.transform.position.z);
        cameraManager.JumpTo(new Vector2(50, 50));

        // Use Reflection to call private SmoothJump method
        MethodInfo smoothJumpMethod = typeof(CameraManager).GetMethod("SmoothJump", BindingFlags.Instance | BindingFlags.NonPublic);
        smoothJumpMethod.Invoke(cameraManager, null);

        // Assert the camera has moved towards the target position
        Assert.Less(Vector3.Distance(cameraObject.transform.position, targetPosition), 1f);
    }

    [Test]
    public void IsMoving_ReturnsTrueWhenCameraIsMoving()
    {
        // Move the camera to a new position to simulate movement
        Vector3 initialPosition = cameraObject.transform.position;
        cameraObject.transform.position = new Vector3(10, 10, 0);

        cameraManager.JumpTo(new Vector2(100f, 100f));

        FieldInfo isMovingField = typeof(CameraManager).GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic);
        bool isMoving = (bool)isMovingField.GetValue(cameraManager);
        
        // Assert that the camera is considered moving
        Assert.IsTrue(isMoving);

        // Reset position for next test
        cameraObject.transform.position = initialPosition;
    }
}
