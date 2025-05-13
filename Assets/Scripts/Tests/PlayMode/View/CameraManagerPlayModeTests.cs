using System.Collections;
using System.Reflection;
using Assets.Scripts.Model.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

public class CameraManagerPlayModeTests
{
    private GameObject cameraObject;
    private CameraManager cameraManager;
    private Camera cam;



    [UnitySetUp]
    public IEnumerator Setup()
    {
        cameraObject = new GameObject();
        cam = cameraObject.AddComponent<Camera>();
        cameraManager = cameraObject.AddComponent<CameraManager>();

        GameObject go = new GameObject();
        SafariMap safariMap = go.AddComponent<SafariMap>();


        safariMap.map_dimensions = new Vector2(100, 100);
        cameraManager.Map = safariMap;

        yield return null; // Wait a frame so Start() runs
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        Object.Destroy(cameraObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SmoothJumpTest()
    {
        // Arrange
        Vector3 startPos = cameraManager.transform.position;
        Vector3 jumpTarget = startPos + new Vector3(10, 0, 0); // Move +10 on x

        typeof(CameraManager)
            .GetField("targetPosition", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(cameraManager, jumpTarget);

        typeof(CameraManager)
            .GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(cameraManager, true);

        // Act
        yield return null; // Frame 1
        yield return null; // Frame 2 (move a bit)

        Vector3 posAfterJump = cameraManager.transform.position;

        // Assert
        Assert.AreNotEqual(startPos, posAfterJump, "Camera should have started moving toward the target.");
    }

    [UnityTest]
    public IEnumerator HandleMovement_IsJumping()
    {
        // Arrange
        typeof(CameraManager)
            .GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(cameraManager, true);

        // Create EventSystem
        if (EventSystem.current == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject selectedObject = new GameObject("Selected");
        EventSystem.current.SetSelectedGameObject(selectedObject);

        typeof(CameraManager)
            .GetMethod("HandleMovement", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(cameraManager, null);

        bool isJumping = (bool)typeof(CameraManager)
            .GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(cameraManager);

        // Assert
        Assert.IsTrue(isJumping, "isJumping should be true, because no input was given.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator HandleMovementTest()
    {
        // Arrange
        Vector3 startPos = cameraManager.transform.position;

        // Fake input? In PlayMode we cannot really set Input.GetAxis directly.
        // So, call HandleMovement() manually if needed

        // Instead, we simulate by calling private HandleMovement directly
        var method = typeof(CameraManager).GetMethod("HandleMovement", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(cameraManager, null);

        // Act
        yield return null;

        // Assert
        Assert.AreEqual(startPos, cameraManager.transform.position, "Without input, camera should not move.");
    }
}
