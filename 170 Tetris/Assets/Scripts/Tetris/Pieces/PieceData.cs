using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PieceData", menuName = "Scriptable Objects/PieceData")]
public class PieceData : ScriptableObject
{
    [SerializeField]
    private Vector2Int center;

    [SerializeField]
    private PieceBlock[] blocks;

    private static readonly Vector2Int[] baseOffsets1 = {new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0)};
    private static readonly Vector2Int[] baseOffsets2 = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2) };
    private static readonly Vector2Int[] baseOffsets3 = { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2) };
    
    //https://tetris.wiki/Super_Rotation_System
    private static readonly WallkickOffests[] defaultWallkickOffsets =
    {
        new WallkickOffests(baseOffsets1),
        new WallkickOffests(baseOffsets2),
        new WallkickOffests(baseOffsets1),
        new WallkickOffests(baseOffsets3),
    };

    [SerializeField]
    private WallkickOffests[] wallkickOffsets = defaultWallkickOffsets;

    #region Editor Menu

    [MenuItem("Assets/Create/Piece")]
    public static void CreatePiece()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Piece", "New Piece", "Asset", "Save Piece", "Assets/Tilemap/Pieces");
        if (path == "")
        {
            return;
        }
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PieceData>(), path);
    }
    #endregion
}

[System.Serializable]
public class PieceBlock
{
    [SerializeField]
    public Vector2Int position;
    [SerializeField]
    private Block block;
}

[System.Serializable]
public class WallkickOffests
{
    [SerializeField]
    public Vector2Int[] offsets;

    public WallkickOffests(Vector2Int[] offsets)
    {
        this.offsets = offsets;
    }
}