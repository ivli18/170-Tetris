using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PieceData", menuName = "Scriptable Objects/PieceData")]
public class PieceData : ScriptableObject
{
    //Define the central pivot of the piece, must be an integer
    [SerializeField]
    private Vector2Int center;

    [SerializeField]
    private PieceBlock[] blocks;

    /*private static readonly Vector2Int[] baseOffsets1 = {new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0)};
    private static readonly Vector2Int[] baseOffsets2 = { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2) };
    private static readonly Vector2Int[] baseOffsets3 = { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2) };
    
    private static readonly WallkickOffests[] defaultWallkickOffsets =
    {
        new WallkickOffests(baseOffsets1),
        new WallkickOffests(baseOffsets2),
        new WallkickOffests(baseOffsets1),
        new WallkickOffests(baseOffsets3),
    };*/

    //https://tetris.wiki/Super_Rotation_System
    //The first offset is always applied (this ensures I and O pieces rotate correctly)
    //and loops through the others if the rotation is obstructed
    [SerializeField]
    private WallkickOffests[] wallkickOffsets;

    public PieceBlock[] GetBlocks()
    {
        return blocks;
    }

    public Vector2Int GetCenter()
    {
        return center;
    }

    public WallkickOffests[] GetWallkickOffests()
    {
        return wallkickOffsets;
    }

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
    public Block block;
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