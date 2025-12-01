using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using Assets.Scripts.Model.Map;
using UnityEngine.AI;
using NavMeshPlus;
using NavMeshPlus.Components;

public class JeepPlayModeTests : MonoBehaviour
{
    private GameObject jeepPrefab;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Load your scene and set up the jeep prefab
        SceneManager.LoadScene("Game");
        yield return null; // Wait a frame for the scene to load

        var gameManager = GameObject.FindObjectOfType<GameManager>();
        gameManager.testMode = true;
        gameManager.jeepCount = 1;
        var map = GameObject.FindObjectOfType<SafariMap>();
        jeepPrefab = map.prefab_jeep;  // Assuming prefab_jeep is publicly accessible

    }

    [UnityTest]
    public IEnumerator JeepIsSpawnedCorrectly()
    {
        yield return null; // Wait a frame for the setup to complete

        var gm = GameObject.FindObjectOfType<GameManager>();

        // Spawn the jeep from the prefab
        GameObject jeep = Instantiate(jeepPrefab, new Vector3(37.5f, 39.5f, 0f), Quaternion.identity);
        var jeepComponent = jeep.GetComponent<Jeep>();

        // Set properties and initialize the jeep
        jeepComponent.SetManager(gm);
        jeepComponent.tourists = 3; // Assign 3 tourists

        // Wait for a frame to let updates happen
        yield return null;

        // Assertions to verify jeep instantiation and state
        Assert.IsNotNull(jeep, "Jeep should have been spawned");
        Assert.AreEqual(3, jeepComponent.tourists, "Jeep should have 3 tourists");
        Assert.GreaterOrEqual(0.1, Vector3.Distance(new Vector3(37.5f, 39.5f, 0f), jeep.transform.position));
    }
    [UnityTest]
    public IEnumerator JeepGetsStuckAndKilled()
    {
        // Setup
        yield return null; // Wait for frame

        var gm = GameObject.FindObjectOfType<GameManager>();
        GameObject jeep = Instantiate(jeepPrefab, new Vector3(37.5f, 39.5f, 0f), Quaternion.identity);
        var jeepComponent = jeep.GetComponent<Jeep>();
        jeepComponent.SetManager(gm);
        jeepComponent.tourists = 3;

        // Make jeep "stuck" by keeping it in place
        jeep.transform.position = new Vector3(37.5f, 39.5f, 0f);

        // Wait for a few seconds to allow stuck check to be triggered
        yield return new WaitForSeconds(10f);

        // Test: Verify jeep is destroyed after being stuck
        Assert.IsTrue(jeep == null);
    }

    [UnityTest]
    public IEnumerator JeepMovesCorrectly()
    {
        // Setup
        yield return null;
        var map = GameObject.FindObjectOfType<SafariMap>().GetComponent<SafariMap>();
        map.ChangeTileToRoad(new Vector2(36.5f, 38f), true); // Ensure the destination is a road tile
        map.ChangeTileToRoad(new Vector2(36.5f, 39f), true); // Ensure the destination is a road tile
        map.ChangeTileToRoad(new Vector2(36.5f, 40f), true); // Ensure the destination is a road tile
        
        yield return null;
        var gm = GameObject.FindObjectOfType<GameManager>();
        gm.navMesh.GetComponent<NavMeshSurface>().UpdateNavMesh(gm.navMesh.GetComponent<NavMeshSurface>().navMeshData); // Rebuild the nav mesh after changing tiles
        GameObject jeep = Instantiate(jeepPrefab, new Vector3(39f, 39.5f, 0f), Quaternion.identity);
        yield return null;
        var jeepComponent = jeep.GetComponent<Jeep>();
        jeepComponent.SetManager(gm);


        yield return new WaitForSeconds(7f);


        // Check if jeep moved (you can modify the distance check if needed)
        Assert.GreaterOrEqual(Vector3.Distance(new Vector2(36.5f, 39.5f), jeep.transform.position),0.5);

    }

    [UnityTest]
    public IEnumerator JeepReachesExitAndReturnsHome()
    {
        // Setup
        yield return null;
        var map = GameObject.FindObjectOfType<SafariMap>().GetComponent<SafariMap>();
        for (int i = 0; i < 4; i++)
        {
            map.ChangeTileToRoad(new Vector2(36.5f, 38f+i), true); // Ensure the destination is a road tile
            map.ChangeTileToRoad(new Vector2(43.5f, 38f+i), true); // Ensure the destination is a road tile
        }
        for (int i = 1; i <= 7; i++)
        {
            map.ChangeTileToRoad(new Vector2(36.5f + i, 41), true); // Ensure the destination is a road tile
        }

        yield return null;
        var gm = GameObject.FindObjectOfType<GameManager>();
        gm.navMesh.GetComponent<NavMeshSurface>().UpdateNavMesh(gm.navMesh.GetComponent<NavMeshSurface>().navMeshData); // Rebuild the nav mesh after changing tiles
        GameObject jeep = Instantiate(jeepPrefab, new Vector3(39f, 39.5f, 0f), Quaternion.identity);
        Assert.IsNotNull(jeep, "Jeep should have been spawned");
        yield return null;
        var jeepComponent = jeep.GetComponent<Jeep>();
        jeepComponent.SetManager(gm);


        yield return new WaitForSeconds(60f);

        Assert.IsTrue(jeep == null); // jeep reached home


    }

    [UnityTest]
    public IEnumerator JeepSpeedChangesProperly()
    {
        // Setup
        yield return null;
        var gm = GameObject.FindObjectOfType<GameManager>();
        GameManager.Instance = gm;
        GameObject jeep = Instantiate(jeepPrefab, new Vector3(39f, 39.5f, 0f), Quaternion.identity);
        Assert.IsNotNull(jeep, "Jeep should have been spawned");
        yield return null;
        var jeepComponent = jeep.GetComponent<Jeep>();
        jeepComponent.SetManager(gm);


        GameManager.Instance.CurrentGameSpeed = GameManager.GameSpeed.Normal;
        yield return null;
        float sp = 0.5f;
        float baseAcceleration = 2f;
        float baseAngularSpeed = 60f;
        float expectedSpeed = Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed * sp, 0.5f, 3.5f);
        float expectedAcceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        float expectedAngularSpeed = baseAngularSpeed * Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed, 1f, 3f);

        Assert.AreEqual(expectedSpeed, jeepComponent.GetComponent<NavMeshAgent>().speed);
        Assert.AreEqual(expectedAcceleration, jeepComponent.GetComponent<NavMeshAgent>().acceleration);
        Assert.AreEqual(expectedAngularSpeed, jeepComponent.GetComponent<NavMeshAgent>().angularSpeed);



        GameManager.Instance.CurrentGameSpeed = GameManager.GameSpeed.Double;
        yield return null;
        expectedSpeed = Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed * sp, 0.5f, 3.5f);
        expectedAcceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        expectedAngularSpeed = baseAngularSpeed * Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed, 1f, 3f);
        
        Assert.AreEqual(expectedSpeed, jeepComponent.GetComponent<NavMeshAgent>().speed);
        Assert.AreEqual(expectedAcceleration, jeepComponent.GetComponent<NavMeshAgent>().acceleration);
        Assert.AreEqual(expectedAngularSpeed, jeepComponent.GetComponent<NavMeshAgent>().angularSpeed);



        GameManager.Instance.CurrentGameSpeed = GameManager.GameSpeed.Triple;
        yield return null;
        expectedSpeed = Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed * sp, 0.5f, 3.5f);
        expectedAcceleration = baseAcceleration * (int)GameManager.Instance.CurrentGameSpeed;
        expectedAngularSpeed = baseAngularSpeed * Mathf.Clamp((int)GameManager.Instance.CurrentGameSpeed, 1f, 3f);

        Assert.AreEqual(expectedSpeed, jeepComponent.GetComponent<NavMeshAgent>().speed);
        Assert.AreEqual(expectedAcceleration, jeepComponent.GetComponent<NavMeshAgent>().acceleration);
        Assert.AreEqual(expectedAngularSpeed, jeepComponent.GetComponent<NavMeshAgent>().angularSpeed);
    }

}
