using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Assets.Scripts.Model.Map;

public class MinimapTests
{
    private GameObject minimapObj;
    private Minimap minimap;
    private Camera mainCamera;
    private Camera minimapCamera;
    private RectTransform minimapFrame;
    private RectTransform minimapUI;
    private SafariMap safariMap;

    [SetUp]
    public void Setup()
    {
        minimapObj = new GameObject();
        minimap = minimapObj.AddComponent<Minimap>();

        // Mock main camera
        var mainCamObj = new GameObject();
        mainCamera = mainCamObj.AddComponent<Camera>();
        minimap.mainCamera = mainCamera;

        // Mock minimap camera
        var minimapCamObj = new GameObject();
        minimapCamera = minimapCamObj.AddComponent<Camera>();
        minimap.minimapCamera = minimapCamera;

        // Mock minimap frame (the rectangle)
        var frameObj = new GameObject();
        minimapFrame = frameObj.AddComponent<RectTransform>();
        minimap.minimapFrame = minimapFrame;

        // Mock minimap UI (the whole minimap)
        var uiObj = new GameObject();
        minimapUI = uiObj.AddComponent<RectTransform>();
        minimap.minimapUI = minimapUI;

        // Mock safari map
        var safariMapObj = new GameObject();
        safariMap = safariMapObj.AddComponent<SafariMap>();
        safariMap.map_dimensions = new Vector2Int(100, 100);
        minimap.safariMap = safariMap;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(minimapObj);
    }

    [Test]
    public void Start_SetsMinimapCameraPropertiesCorrectly()
    {
        // Act
        MethodInfo startMethod = typeof(Minimap).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(minimap, null);

        // Assert
        Assert.AreEqual(50f, minimap.minimapCamera.orthographicSize);
        Assert.AreEqual(new Vector3(47.8f, 50.9f, -10f), minimap.minimapCamera.transform.position);
    }

    [Test]
    public void Update_UpdatesMinimapFramePositionAndSize()
    {
        // Arrange
        MethodInfo startMethod = typeof(Minimap).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(minimap, null); // Kell hogy legyen mapSize beállítva
        mainCamera.transform.position = new Vector3(50, 50, -10);
        mainCamera.orthographicSize = 5;
        mainCamera.aspect = 1.5f;

        minimapUI.sizeDelta = new Vector2(200, 200); // Minimap UI mérete

        // Act
        MethodInfo updateMethod = typeof(Minimap).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        updateMethod.Invoke(minimap, null);

        // Assert - Check the minimapFrame position
        Vector2 expectedPosition = new Vector2(9f, -5f); // középen (50/100) * UI méret = 100, mínusz UI fele 100/2 = 50 => 50 offset, plusz hardkódolt 9/-5
        Assert.AreEqual(expectedPosition.x, minimap.minimapFrame.anchoredPosition.x, 1f); // 1 pixel tolerancia
        Assert.AreEqual(expectedPosition.y, minimap.minimapFrame.anchoredPosition.y, 1f);

        // Assert - Check the minimapFrame size
        float expectedWidth = (mainCamera.orthographicSize * 2 * mainCamera.aspect) / 100 * 200;
        float expectedHeight = (mainCamera.orthographicSize * 2) / 100 * 200;
        Assert.AreEqual(expectedWidth, minimap.minimapFrame.sizeDelta.x, 0.1f);
        Assert.AreEqual(expectedHeight, minimap.minimapFrame.sizeDelta.y, 0.1f);
    }

    [Test]
    public void Update_DoesNothing_WhenMainCameraOrUIIsMissing()
    {
        // Arrange
        minimap.mainCamera = null; // Null main camera
        minimap.minimapUI = null;  // Null minimap UI

        // Mentsük el az eredeti pozíciót és méretet a minimapFrame-hez
        Vector2 initialPosition = minimap.minimapFrame.anchoredPosition;
        Vector2 initialSize = minimap.minimapFrame.sizeDelta;

        // Act
        MethodInfo updateMethod = typeof(Minimap).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
        updateMethod.Invoke(minimap, null);

        // Assert - Ellenõrizzük, hogy semmi nem változott
        Assert.AreEqual(initialPosition, minimap.minimapFrame.anchoredPosition);
        Assert.AreEqual(initialSize, minimap.minimapFrame.sizeDelta);
    }

}
