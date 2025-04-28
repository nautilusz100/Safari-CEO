using System.Collections;
using Assets.Scripts.Model.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class DraggableTests
{
    private GameObject canvasObject;
    private GameObject draggableObject;
    private Draggable draggable;
    private SafariMap fakeMap;
    private GameManager fakeGameManager;
    private GameObject eventSystem;

    [SetUp]
    public void SetUp()
    {

        var cameraGameObject = new GameObject("MainCamera");
        var camera = cameraGameObject.AddComponent<Camera>();
        cameraGameObject.tag = "MainCamera";
        // Create a fake Canvas
        canvasObject = new GameObject("Canvas", typeof(Canvas));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        // Create a fake EventSystem
        eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Create Draggable object
        draggableObject = new GameObject("Draggable", typeof(Image), typeof(Draggable));
        draggable = draggableObject.GetComponent<Draggable>();
        draggable.canvas = canvasObject.GetComponent<Canvas>();
        var uiDocument = draggableObject.AddComponent<UIDocument>();


        draggable.buttonsManager = draggableObject.AddComponent<gameUIButtonsManager>();
        draggable.buttonsManager.TestMode();

        typeof(gameUIButtonsManager)
        .GetField("currentShopType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(draggable.buttonsManager, ShopTypes.ANIMALS);


        // Set fake animal prefabs
        draggable.foxPrefab = new GameObject("FoxPrefab");
        draggable.lionPrefab = new GameObject("LionPrefab");
        draggable.giraffePrefab = new GameObject("GiraffePrefab");
        draggable.zebraPrefab = new GameObject("ZebraPrefab");
        draggable.prefabToSpawn = draggable.foxPrefab;

        // Set a fake SafariMap
        fakeMap = new GameObject().AddComponent<SafariMap>();
        draggable.map = fakeMap;

        // Create a fake GameManager
        fakeGameManager = new GameObject("GameManager").AddComponent<GameManager>();
        GameManager.Instance = fakeGameManager;
        fakeGameManager.testMode = true;
        fakeGameManager.Money = 10000; // enough money
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(canvasObject);
        Object.DestroyImmediate(draggableObject);
        Object.DestroyImmediate(eventSystem);
        Object.DestroyImmediate(fakeGameManager.gameObject);
    }

    [UnityTest]
    public IEnumerator OnBeginDrag_CreatesGhost_WhenEnoughMoney()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        draggable.OnBeginDrag(eventData);

        yield return null;

        Assert.IsNotNull(GameObject.Find("Ghost"));
    }

    [UnityTest]
    public IEnumerator OnBeginDrag_DoesNotCreateGhost_WhenNotEnoughMoney()
    {
        GameManager.Instance.Money = 0; // No money
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        draggable.OnBeginDrag(eventData);

        yield return null;

        Assert.IsNull(GameObject.Find("Ghost"));
    }

    [UnityTest]
    public IEnumerator OnDrag_UpdatesGhostPosition()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(100, 100)
        };

        draggable.OnBeginDrag(eventData);

        yield return null;

        var ghost = GameObject.Find("Ghost");
        Vector3 before = ghost.transform.position;

        eventData.position = new Vector2(200, 200);
        draggable.OnDrag(eventData);

        yield return null;

        Vector3 after = ghost.transform.position;

        Assert.AreNotEqual(before, after);
    }

    [UnityTest]
    public IEnumerator OnEndDrag_DestroysGhost()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        draggable.OnBeginDrag(eventData);
        
        yield return null;

        Assert.IsNotNull(GameObject.Find("Ghost"));
        typeof(gameUIButtonsManager)
        .GetField("currentShopType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .SetValue(draggable.buttonsManager, ShopTypes.JEEP);

        draggable.OnEndDrag(eventData);

        yield return null;

        Assert.IsNull(GameObject.Find("Ghost"));
    }

    [Test]
    public void Start_AssignsAnimalType_Fox()
    {
        var draggableObject = new GameObject().AddComponent<Draggable>();

        // Setup: create dummy prefabs
        var foxPrefab = new GameObject("FoxPrefab");
        draggableObject.foxPrefab = foxPrefab;
        draggableObject.prefabToSpawn = foxPrefab;

        typeof(Draggable)
            .GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(draggable, null);

        Draggable.AnimalType animtyp = (Draggable.AnimalType) typeof(Draggable)
        .GetField("animalType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .GetValue(draggable);
        Assert.AreEqual(animtyp,Draggable.AnimalType.Fox);
    }


    [UnityTest]
    public IEnumerator Update_ScalesGhostBasedOnCameraZoom()
    {
        var cameraObj = new GameObject("MainCamera", typeof(Camera));
        cameraObj.tag = "MainCamera";
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(100, 100)
        };

        draggable.OnBeginDrag(eventData);

        yield return null;

        var ghost = GameObject.Find("Ghost");

        Vector3 initialScale = ghost.transform.localScale;

        Camera.main.orthographicSize = 2f;

        yield return null; // Let Update() happen

        Vector3 newScale = ghost.transform.localScale;

        Assert.AreNotEqual(initialScale, newScale);

        Object.DestroyImmediate(cameraObj);
    }
}
