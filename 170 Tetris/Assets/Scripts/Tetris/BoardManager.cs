using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    private Grid grid;
    private Tilemap background;
    public Tilemap blocks;
    [SerializeField]
    private Block tile;

    private int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    private int boardWidth = 10; // hesitant to say this should be changable


    void Awake()
    {
        grid = GetComponent<Grid>();
        background = transform.Find("Background").GetComponent<Tilemap>();
        blocks = transform.Find("Blocks").GetComponent<Tilemap>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blocks.SetTile(new Vector3Int(0, 0, 0), tile);
        blocks.SetTile(new Vector3Int(0, 20, 0), tile);
        blocks.SetTile(new Vector3Int(9, 0, 0), tile);
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
        }
    }

    public void CheckLineClear()
    {

    }

    private void ClearLine()
    {

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
