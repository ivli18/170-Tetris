using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Block : TileBase
{
    [SerializeField]
    private int score = 1;

    //public UnityEvent onPlace;
    //public UnityEvent onDestroy;
    //public UnityEvent<int> onScore;

    [SerializeField]
    private Sprite sprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
    }

    public void PlaceBlock()
    {
        //onPlace.Invoke();
    }

    public int ScoreBlock(int mult)
    {
        //onScore.Invoke(score * mult);
        return score * mult;
    }

    public void DestroyBlock()
    {
        //onDestroy.Invoke();
    }

    #region Editor Menu
    #if UNITY_EDITOR

    [MenuItem("Assets/Create/Block")]
    public static void CreateBlock()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Block", "New Block", "Asset", "Save Block", "Assets/Tilemap/Blocks");
        if (path == "")
        {
            return;
        }
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Block>(), path);
    }
    #endif
    #endregion
}
