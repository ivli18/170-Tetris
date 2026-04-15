using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    private Grid grid;
    private Tilemap background;
    public Tilemap blocks;
    [SerializeField]
    private Tile tileBackground;
    private SpriteRenderer inactiveOverlay;

    public int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    public int boardWidth = 10; // hesitant to say this should be changable

    private static int[] LINE_CLEAR_MULTIS = { 10, 15, 20, 25, 30 };
    private static int[] LINE_CLEAR_POINTS = { 10, 30, 50, 80, 160 };


    void Awake()
    {
        grid = GetComponent<Grid>();
        background = transform.Find("Background").GetComponent<Tilemap>();
        blocks = transform.Find("Blocks").GetComponent<Tilemap>();
        inactiveOverlay = transform.Find("InactiveOverlay").GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetupBoard();
    }

    public void SetupBoard()
    {
        for(int i = 0; i < boardWidth; i++)
        {
            for( int k = 0; k < boardHeight; k++)
            {
                background.SetTile(new Vector3Int(i, k, 0), tileBackground);
                if((i + k) % 2 == 0)
                {
                    background.SetColor(new Vector3Int(i, k, 0), Color.gray1);
                }
                else
                {
                    background.SetColor(new Vector3Int(i, k, 0), Color.gray2);
                }
            }
        }

        inactiveOverlay.transform.localPosition = new Vector3(boardWidth / 2.0f, boardHeight / 2.0f, 0);
        inactiveOverlay.transform.localScale = new Vector3(boardWidth, boardHeight, 1);
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
            if (piece.position.y + block.position.y < boardHeight)
            {
                blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.color);
            }
            else
            {
                blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), Color.clear);
            }
        }
    }

    public void DrawGhostPiece(Piece piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.block);
            blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), new Color(block.color.r - 0.5f, block.color.g - 0.5f, block.color.b - 0.5f, 0.5f));
        }
    }

    public void DrawSwapGhostPiece(Piece piece)
    {
        if (piece == null) { return; }

        bool emptySpace = piece.CheckForEmptySpace(new Vector2Int(0, 0), this);

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            if (blocks.GetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0)) == null && ((piece.position.y + block.position.y) < boardHeight))
            {
                blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), block.block);
                if (emptySpace)
                {
                    blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), new Color(0.0f, 1.0f, 0.0f, 0.5f));
                }
                else
                {
                    blocks.SetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), new Color(1.0f, 0.0f, 0.0f, 0.5f));
                }
            }
        }
    }

    public void ClearSwapGhostPiece(Piece piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.pieceData.GetBlocks())
        {
            if (blocks.GetColor(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0)).a < 1.0f)
            {
                blocks.SetTile(new Vector3Int(piece.position.x + block.position.x, piece.position.y + block.position.y, 0), null);
            }
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
            //score = LINE_CLEAR_POINTS[Math.Min(rowsToClear.Count - 1, LINE_CLEAR_POINTS.Length - 1)];
            score = rowsToClear.Count;
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

    public bool PositionInBounds(Vector2Int position)
    {
        if(position.x >= 0 && position.x < boardWidth && position.y >= 0)
        {
            return true;
        }

        return false;
    }

    public void SetActive()
    {
        inactiveOverlay.enabled = false;
    }

    public void SetInactive()
    {
        inactiveOverlay.enabled = true;
    }

    public void BoardLoss()
    {
        blocks.color = Color.black;
        SetInactive();
    }
}
