using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
/// <summary>
/// Data class for allowing a cell to echo when interacting with it.
/// </summary>
public struct CellData
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CellData (Vector3 localPosition, Vector2 coords, bool isMarked)
    {
        LocalPosition = localPosition;
        Coords = coords;
        IsMarked = isMarked;
    }

    public Vector3 LocalPosition;
    /// <summary>
    /// X for Row and Y for Column
    /// </summary>
    public Vector2 Coords;
    /// <summary>
    /// Is already marked
    /// </summary>
    public bool IsMarked;
}
