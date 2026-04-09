using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    private Grid grid;
    private Tilemap UI;
    private Tilemap background;
    public Tilemap blocks;
    [SerializeField]
    private Block tile;

    public int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    public int boardWidth = 10; // hesitant to say this should be changable

    private static int[] LINE_CLEAR_MULTIS = { 10, 15, 20, 25, 30 };
    private static int[] LINE_CLEAR_POINTS = { 10, 30, 50, 80, 160 };


    void Awake()
    {
        grid = GetComponent<Grid>();
        background = transform.Find("Background").GetComponent<Tilemap>();
        blocks = transform.Find("Blocks").GetComponent<Tilemap>();
        UI = transform.Find("UITilemap").GetComponent<Tilemap>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupBoard()
    {
        // set up the grid size
    }

    private void ClearBoard()
    {

    }

    public void SpawnPiece(Piece piece)
    {

    }

    public void ClearPiece(Piece piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), null);
        }
    }

    public void DrawPiece(Piece piece)
    {
        if(piece == null) { return; }

        foreach(PieceBlock block in piece.pieceData.GetBlocks())
        {
            blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.block);
            blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.color);
        }
    }

    public void DrawGhostPiece(Piece piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.block);
            blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), new Color(block.color.r, block.color.g, block.color.b, 0.5f));
        }
    }

    public void DrawHeldPiece(Piece piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            UI.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.block);
            UI.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), new Color(block.color.r, block.color.g, block.color.b, 0.5f));
        }
    }

    public int CheckLineClear()
    {
        Queue<int> rowsToClear = new Queue<int>();
        int score = 0;
        for (int i = boardHeight; i >= 0; i--)
        {
            if(CheckRowFull(i))
            {
                rowsToClear.Enqueue(i);
            }
        }

        if (rowsToClear.Count != 0)
        {
            score = LINE_CLEAR_POINTS[Math.Min(rowsToClear.Count - 1, LINE_CLEAR_POINTS.Length - 1)];
            //int scoreMultiplier = LINE_CLEAR_MULTIS[Math.Min(rowsToClear.Count - 1, LINE_CLEAR_MULTIS.Length - 1)]; -- MULTCLEAR
            while (rowsToClear.Count != 0)
            {
                //score += ClearLine(rowsToClear.Dequeue(), 1); -- MULTCLEAR
                ClearLine(rowsToClear.Dequeue(), 1);
            }
        }
        return score;
    }

    private bool CheckRowFull(int row)
    {
        for(int i = 0; i < boardWidth; i++)
        {
            if(blocks.GetTile(new Vector3Int(i, row, 0)) == null)
            {
                return false;
            }
        }

        return true;
    }

    private int ClearLine(int row, int scoreMultiplier)
    {
        int score = 0;
        for (int i = 0; i < boardWidth; i++)
        {
            Block block = (Block)(blocks.GetTile(new Vector3Int(i, row, 0)));
            //score += block.ScoreBlock(scoreMultiplier); -- MULTCLEAR
            blocks.SetTile(new Vector3Int(i, row, 0), null);
        }

        for (int i = row; i < boardHeight + 5; i++)
        {
            for (int k = 0; k < boardWidth; k++)
            {
                TileBase tile = blocks.GetTile(new Vector3Int(k, i + 1, 0));
                if (tile != null)
                {
                    Color color = blocks.GetColor(new Vector3Int(k, i + 1, 0));
                    blocks.SetTile(new Vector3Int(k, i), tile);
                    blocks.SetColor(new Vector3Int(k, i), color);
                    blocks.SetTile(new Vector3Int(k, i + 1), null);

                }
            }
        }
        return score;
    }

    private void ClearTile(Vector2Int tile)
    {

    }

    private void ScoreTile(Vector2Int tile)
    {
        
    }

    public bool PositionInBounds(Vector2Int position)
    {
        if(position.x >= 0 && position.x < boardWidth && position.y >= 0)
        {
            return true;
        }

        return false;
    }
}
