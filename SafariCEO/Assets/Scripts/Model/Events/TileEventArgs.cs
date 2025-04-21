using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEventArgs : EventArgs
{
    public Vector2Int Position { get; }

    public TileEventArgs(Vector2Int position)
    {
        Position = position;
    }
}
