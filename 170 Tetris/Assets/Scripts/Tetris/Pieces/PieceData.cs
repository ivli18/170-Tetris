using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Rarity { D, C, B, A, S, SS};

[CreateAssetMenu(fileName = "PieceData", menuName = "Scriptable Objects/PieceData")]
public class PieceData : ScriptableObject
{
    /* HOW TO MAKE A PIECE
     * - duplicate one of the existing pieces (other than O and I in order to have rotation data correct)
     * - set blocks in the array
     * - IMPORTANT: (0, 0) on a block is the BOTTOM LEFT position; positive integers for x/y are right/up repsectively
     * - set the block asset (new blocks can be made from the create menu or duplicating an existing one)
     * - if you want to make a special block, make a scriptable object that derrives from the Block class (ill make an example one at some point)
     * - set the center pivot; this determines where the piece rotates from. the center must be a full integer, rotation offsets must be used to correct decimal centers (like O and I).
     */
    
    //Define the central pivot of the piece, must be an integer
    [SerializeField]
    private Vector2Int center;

    [SerializeField]
    private PieceBlock[] blocks;


    [SerializeField]
    private Rarity rarity;

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

    public Rarity GetRarity()
    {
        return rarity;
    }

    public WallkickOffests[] GetWallkickOffests()
    {
        return wallkickOffsets;
    }

    #region Editor Menu
    #if UNITY_EDITOR

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
    #endif
    #endregion
}

[System.Serializable]
public class PieceBlock
{
    [SerializeField]
    public Vector2Int position;
    [SerializeField]
    public Block block;
    [SerializeField]
    public Color color = Color.white;
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