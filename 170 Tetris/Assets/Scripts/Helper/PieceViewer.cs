using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.Collections.AllocatorManager;

public class PieceViewer : MonoBehaviour
{
    public PieceCollection allPieces;
    private Grid grid;
    private Tilemap tilemap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GetComponent<Grid>();
        tilemap = transform.Find("Tilemap").GetComponent<Tilemap>();

        for (int i = 0; i < allPieces.pieces.Count; i++)
        {
            DrawPiece(allPieces.pieces[i], new Vector2Int(((i % 10) * 6), (Mathf.FloorToInt(i / 10.0f) * 6)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawPiece(PieceData piece, Vector2Int offset)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.GetBlocks())
        {
            tilemap.SetTile(new Vector3Int(offset.x + block.position.x, offset.y + block.position.y, 0), block.block);
            tilemap.SetColor(new Vector3Int(offset.x + block.position.x, offset.y + block.position.y, 0), new Color(block.color.r, block.color.g, block.color.b, 1.0f));
        }

        tilemap.SetColor(new Vector3Int(offset.x + piece.GetCenter().x, offset.y + piece.GetCenter().y, 0), Color.black);
    }
}
