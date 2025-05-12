using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LoadSettings class to manage the loading path.
/// <summary>
public static class LoadSettings
{
    public static string LoadPath;
    public static bool IsLoadRequested => !string.IsNullOrEmpty(LoadPath);
}

