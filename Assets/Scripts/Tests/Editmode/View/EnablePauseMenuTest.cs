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
        // Set initial state
        pauseMenuObject.SetActive(false);
        Assert.IsFalse(pauseMenuObject.activeSelf, "Pause menu should start inactive");

        // Act: toggle using the actual method
        var toggleMethod = typeof(PauseMenuController).GetMethod("TogglePauseMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        toggleMethod.Invoke(enablePauseMenuComponent, null);

        // Assert: should now be active
        Assert.IsTrue(pauseMenuObject.activeSelf, "Pause menu should be active after toggle");

        // Toggle again
        toggleMethod.Invoke(enablePauseMenuComponent, null);
        Assert.IsFalse(pauseMenuObject.activeSelf, "Pause menu should be inactive after second toggle");
    }

}