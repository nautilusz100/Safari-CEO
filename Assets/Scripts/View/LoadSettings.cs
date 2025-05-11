using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoadSettings
{
    public static string LoadPath;
    public static bool IsLoadRequested => !string.IsNullOrEmpty(LoadPath);
}

