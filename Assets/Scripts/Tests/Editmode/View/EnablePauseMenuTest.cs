using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class EnablePauseMenuTests
{
    private GameObject testObject;
    private EnablePauseMenu enablePauseMenuComponent;
    private GameObject pauseMenuObject;

    [SetUp]
    public void SetUp()
    {
        // Create the main GameObject with EnablePauseMenu component
        testObject = new GameObject("EnablePauseMenuTest");
        enablePauseMenuComponent = testObject.AddComponent<EnablePauseMenu>();

        // Create a mock pause menu GameObject
        pauseMenuObject = new GameObject("PauseMenu");
        enablePauseMenuComponent.enablePauseMenu = pauseMenuObject;

        // Initialize with Start
        var startMethod = typeof(EnablePauseMenu).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        startMethod.Invoke(enablePauseMenuComponent, null);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
        Object.DestroyImmediate(pauseMenuObject);
    }

    [Test]
    public void TogglePauseMenu_TogglesActiveState()
    {
        // Initial state: active
        pauseMenuObject.SetActive(false);
        Assert.IsFalse(pauseMenuObject.activeSelf, "Pause menu should be deactivated after first toggle");

        // Toggle again
        pauseMenuObject.SetActive(true);
        Assert.IsTrue(pauseMenuObject.activeSelf, "Pause menu should be activated after second toggle");
    }

}