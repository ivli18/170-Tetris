using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Audio.ProcessorInstance;
using UnityEngine.Tilemaps;
using TMPro;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    private bool shopOpen = false;
    private bool paused = false;
    private int level = 1;
    private int points = 0;
    private int levelRequirement = 1;
    private int lineClears = 0;
    private static int[] LINE_CLEAR_POINTS = { 10, 30, 50, 80, 160 };

    [SerializeField]
    private List<BoardManager> boards;
    private int activeBoard = 0;
    [SerializeField]
    private List<PieceData> bagFull;
    private List<PieceData> bagCurrent = new List<PieceData>();
    private List<PieceData> previewPieces = new List<PieceData>();
    private Piece activePiece = null;
    private Piece ghostPiece = null;
    private PieceData heldPiece = null;
    private bool holdUsed = false;

    private int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    private int boardWidth = 10; // hesitant to say this should be changable
    private float spawnDelayMax = 6.0f; //figure out timing for all of these, floats may be more appropriate; also known as ARE
    private float spawnDelay = 0.0f;
    private float gravity = 0.016f; // for consistency gravity matches standard tetris gravity notation where 1G = 1 cell per frame (60fps)
    private float gravityApplied = 0.0f;
    private float lockDelay = 30.0f; //base tetris is 0.5 seconds
    private float lockDelayCurrent = 0.0f;
    private int previewPieceCount = 3;
    // NOTE FOR ME: MOVE RESET SAYS YOU MAY ONLY STALL FOR 15 MOVES/ROTATIONS BEFORE LOCKING, MOVING DOWN RESETS THIS
    private int stallMoves = 0;
    private static readonly int STALL_MOVES_MAX = 15;
    private float autoshiftDelay = 0.0f; // if any L/R input pressed last frame, reset to zero
    private static readonly float AUTOSHIFT_DELAY = 10.0f;
    private float autorepeatRate = 0.0f;
    private static readonly float AUTOREPEAT_RATE = 2.0f;
    private int combo = -1;

    private InputAction actionShiftLeft;
    private InputAction actionShiftRight;
    private InputAction actionHardDrop;
    private InputAction actionSoftDrop;
    private InputAction actionRotateClockwise;
    private InputAction actionRotateCounterclockwise;
    private InputAction actionHold;
    private InputAction actionsSwitchBoardLeft;
    private InputAction actionsSwitchBoardRight;
    private InputAction actionsPause;

    public Tilemap heldPieceUI;
    public TextMeshProUGUI currencyTextValue;
    public Tilemap previewPieceUI;
    
    public GameObject PauseScreen;
    private SpriteRenderer PauseSprite;
    public GameObject ShopUI;
    private Animator shopAnim;
    
    public AudioManager audioManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        actionShiftLeft = InputSystem.actions.FindAction("ShiftLeft");
        actionShiftRight = InputSystem.actions.FindAction("ShiftRight");
        actionHardDrop = InputSystem.actions.FindAction("HardDrop");
        actionSoftDrop = InputSystem.actions.FindAction("SoftDrop");
        actionRotateClockwise = InputSystem.actions.FindAction("RotateClockwise");
        actionRotateCounterclockwise = InputSystem.actions.FindAction("RotateCounterclockwise");
        actionHold = InputSystem.actions.FindAction("Hold");
        actionsPause = InputSystem.actions.FindAction("Pause");

        currencyTextValue.SetText(points.ToString());

        ShufflePieces();
        for(int i = 0; i < previewPieceCount; i++)
        {
            previewPieces.Add(bagCurrent[0]);
            bagCurrent.RemoveAt(0);
            if(bagCurrent.Count <= 0)
            {
                ShufflePieces();
            }
        }
        DrawPreviewPieces();

        PauseSprite = PauseScreen.GetComponent<SpriteRenderer>();
        shopAnim = ShopUI.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!paused && !shopOpen)
        {
            RunGameLogic();
        }
        else if(paused)
        {
            if(actionsPause.WasPressedThisFrame()) {
                Unpause();
            }
        }
    }

    private void RunGameLogic()
    {
        if (activePiece == null)
        {
            spawnDelay++;

            if (spawnDelay >= spawnDelayMax)
            {
                SpawnPiece();
                spawnDelay = 0;
            }
        }
        else
        {
            ClearGhostPiece();
            boards[activeBoard].ClearPiece(activePiece);

            if (actionHold.WasPressedThisFrame())
            {
                if (!holdUsed)
                {
                    holdUsed = true;
                    Piece storedPiece = activePiece;
                    if (heldPiece != null)
                    {
                        ClearHeldPiece(heldPiece);
                        SpawnPiece(heldPiece);
                    }
                    else
                    {
                        SpawnPiece();
                    }
                    storedPiece.ResetPieceData();
                    heldPiece = storedPiece.pieceData;
                    DrawHeldPiece(heldPiece); //added ui piece
                    return;
                }
            }

            if (actionHardDrop.WasPressedThisFrame())
            {
                HardDrop();
                return;
            }

            if (actionsPause.WasPressedThisFrame())
            {
                Pause();
            }
            InputLR();
            InputRotate();

            //check to see if space below active piece is occupied
            if (activePiece.CheckForEmptySpace(Vector2Int.down))
            {
                if (actionSoftDrop.WasPressedThisFrame())
                {
                    gravityApplied += Mathf.Max(1.0f, gravity);
                }
                else if (actionSoftDrop.IsPressed())
                {
                    gravityApplied += gravity * 9.0f;
                }

                gravityApplied += gravity;

                while (gravityApplied >= 1.0f)
                {
                    activePiece.Move(Vector2Int.down);
                    gravityApplied -= 1.0f;
                    lockDelayCurrent = 0.0f;
                    stallMoves = 0;
                }
            }
            else
            {
                lockDelayCurrent++;

                if (lockDelayCurrent >= lockDelay)
                {
                    PlacePiece();
                }
            }

            DrawGhostPiece();
            boards[activeBoard].DrawPiece(activePiece);
        }
    }

    private void InputLR()
    {
        // input left
        if (actionShiftLeft.WasPressedThisFrame())
        {
            autoshiftDelay = 0.0f;
            autorepeatRate = 0.0f;
            if (activePiece.Move(Vector2Int.left))
            {
                CheckStallMoves();
            }
        }
        else if (actionShiftLeft.IsPressed())
        {
            autoshiftDelay += 1.0f;
            if (autoshiftDelay >= AUTOSHIFT_DELAY)
            {
                autorepeatRate += 1.0f;
                if (autorepeatRate >= AUTOREPEAT_RATE)
                {
                    if (activePiece.Move(Vector2Int.left))
                    {
                        CheckStallMoves();
                    }
                    autorepeatRate = 0.0f;
                }
            }
        }

        // input right
        if (actionShiftRight.WasPressedThisFrame())
        {
            autoshiftDelay = 0.0f;
            autorepeatRate = 0.0f;
            if (activePiece.Move(Vector2Int.right))
            {
                CheckStallMoves();
            }
        }
        else if (actionShiftRight.IsPressed())
        {
            autoshiftDelay += 1.0f;
            if (autoshiftDelay >= AUTOSHIFT_DELAY)
            {
                if (autoshiftDelay >= AUTOSHIFT_DELAY)
                {
                    autorepeatRate += 1.0f;
                    if (autorepeatRate >= AUTOREPEAT_RATE)
                    {
                        if (activePiece.Move(Vector2Int.right))
                        {
                            CheckStallMoves();
                        }
                        autorepeatRate = 0.0f;
                    }
                }
            }
        }
    }

    private void InputRotate()
    {
        //rotate clockwise
        if (actionRotateClockwise.WasPressedThisFrame())
        {
            if (activePiece.Rotate(1))
            {
                CheckStallMoves();
            }
        }
        else if (actionRotateCounterclockwise.WasPressedThisFrame()) //rotate counterclockwise
        {
            if (activePiece.Rotate(-1))
            {
                CheckStallMoves();
            }
        }
    }

    private void SpawnPiece()
    {
        ClearPreviewPieces();
        SpawnPiece(previewPieces[0]);
        previewPieces.RemoveAt(0);

        previewPieces.Add(bagCurrent[0]);
        bagCurrent.RemoveAt(0);
        DrawPreviewPieces();

        if (bagCurrent.Count == 0)
        {
            ShufflePieces();
        }
    }

    private void SpawnPiece(PieceData piece)
    {
        lockDelayCurrent = 0.0f;
        stallMoves = 0;
        gravityApplied = 0.0f;

        activePiece = new Piece();
        activePiece.board = boards[activeBoard];
        activePiece.pieceData = Instantiate(piece);

        ghostPiece = new Piece();
        ghostPiece.board = boards[activeBoard];
        ghostPiece.pieceData = Instantiate(piece);

        activePiece.position = new Vector2Int(4, boardHeight) - activePiece.pieceData.GetCenter();
        ghostPiece.position = activePiece.position;
        boards[activeBoard].SpawnPiece(activePiece);

        if (!activePiece.CheckForEmptySpace(Vector2Int.zero))
        {
            BoardLoss();
        }
    }

    private void ShufflePieces()
    {
        if (bagCurrent.Count > 0)
        {
            bagCurrent.Clear();
        }

        foreach (PieceData piece in bagFull)
        {
            bagCurrent.Insert(UnityEngine.Random.Range(0, bagCurrent.Count + 1), piece);
        }
    }

    private void PlacePiece()
    {
        boards[activeBoard].DrawPiece(activePiece);
        if (!activePiece.CheckWithinBoard())
        {
            BoardLoss();
            return;
        }
        activePiece = null;
        int linesCleared = boards[activeBoard].CheckLineClear();
        if (linesCleared != 0)
        {
            int pointsToAdd = LINE_CLEAR_POINTS[Math.Min(linesCleared - 1, LINE_CLEAR_POINTS.Length - 1)];
            points += pointsToAdd;
            combo += 1;
            lineClears += linesCleared;
            print("LINE CLEAR: " + (points - pointsToAdd) + " + " + pointsToAdd + " = " + points);
            currencyTextValue.SetText(points.ToString());
            audioManager.PlaySoundClear();
            if (lineClears >= levelRequirement)
            {
                LevelClear();
            }
        }
        else
        {
            if(combo > 0)
            {
                points += (combo * 5);
                print("COMBO: " + (points - (combo * 5)) + " + " + (combo * 5) + " = " + points);
                currencyTextValue.SetText(points.ToString());
            }
            combo = -1;
        }
        holdUsed = false;
        audioManager.PlaySoundPlace();
    }

    private void HardDrop()
    {
        while(activePiece.Move(Vector2Int.down)) { }
        PlacePiece();
    }

    private void CheckStallMoves()
    {
        if (stallMoves <= STALL_MOVES_MAX)
        {
            lockDelayCurrent = 0.0f;
            stallMoves++;
        }
    }

    private void BoardLoss()
    {
        boards[activeBoard].blocks.ClearAllTiles();
        SpawnPiece();
    }

    private void GameOver()
    {
        // called when all boards are lost
    }

    private void DrawGhostPiece()
    {
        if (activePiece != null)
        {
            ghostPiece.pieceData = activePiece.pieceData;
            ghostPiece.position = activePiece.position;
            while (ghostPiece.Move(Vector2Int.down)) { }
            boards[activeBoard].DrawGhostPiece(ghostPiece);
        }
    }

    private void ClearGhostPiece()
    {
        boards[activeBoard].ClearPiece(ghostPiece);
    }

    public void AddPieceToBag(PieceData piece)
    {
        bagFull.Add(piece);
        ShufflePieces();
    }

    public void DrawHeldPiece(PieceData piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.GetBlocks())
        {
            heldPieceUI.SetTile(new Vector3Int(block.position.x - piece.GetCenter().x, block.position.y - piece.GetCenter().y, 0), block.block);
            heldPieceUI.SetColor(new Vector3Int(block.position.x - piece.GetCenter().x, block.position.y - piece.GetCenter().y, 0), block.color);
        }
    }

    public void ClearHeldPiece(PieceData piece)
    {
        if (piece == null) { return; }

        foreach (PieceBlock block in piece.GetBlocks())
        {
            heldPieceUI.SetTile(new Vector3Int(block.position.x - piece.GetCenter().x, block.position.y - piece.GetCenter().y, 0), null);
        }
    }

    public void DrawPreviewPieces()
    {
        for(int i = 0; i < previewPieceCount; i++)
        {
            foreach (PieceBlock block in previewPieces[i].GetBlocks())
            {
                previewPieceUI.SetTile(new Vector3Int(block.position.x - previewPieces[i].GetCenter().x, block.position.y - previewPieces[i].GetCenter().y + (12 - i * 4), 0), block.block);
                previewPieceUI.SetColor(new Vector3Int(block.position.x - previewPieces[i].GetCenter().x, block.position.y - previewPieces[i].GetCenter().y + (12 - i * 4), 0), block.color);
            }
        }
    }

    public void ClearPreviewPieces()
    {
        previewPieceUI.ClearAllTiles();
    }

    private void LevelClear()
    {
        level += 1;
        //open shop
        shopAnim.Play("shopOpen");
    }

    private void Pause()
    {
        paused = true;
        PauseSprite.enabled = true;
    }

    private void Unpause()
    {
        paused = false;
        PauseSprite.enabled = false;
    }
}

public class Piece
{
    private int dir = 0;
    public Vector2Int position = new Vector2Int(0, 0);
    public PieceData pieceData;
    public BoardManager board;

    public void Step()
    {

    }

    public bool Rotate(int dir)
    {
        RotateBlocks(dir);

        int i = 0;
        Vector2Int[] currentOffsets = pieceData.GetWallkickOffests()[this.dir].offsets;
        Vector2Int[] rotateOffsets = pieceData.GetWallkickOffests()[(this.dir + dir + 4) % 4].offsets;
        while (Move(currentOffsets[i] - rotateOffsets[i]) == false)
        {
            i++;
            if (i >= currentOffsets.Length)
            {
                RotateBlocks(-dir);
                return false;
            }
        }

        this.dir = (this.dir + dir + 4) % 4;
        return true;
    }

    private void RotateBlocks(int dir)
    {
        foreach(PieceBlock block in pieceData.GetBlocks())
        {
            block.position -= pieceData.GetCenter();
            block.position = new Vector2Int(block.position.y, -block.position.x) * dir;
            block.position += pieceData.GetCenter();
        }
    }

    public bool Move(Vector2Int offset)
    {
        if(CheckForEmptySpace(offset))
        {
            position = position + offset;
            return true;
        }

        return false;
    }

    public bool CheckForEmptySpace(Vector2Int offset)
    {
        foreach(PieceBlock block in pieceData.GetBlocks())
        {
            if(board.blocks.GetTile(new Vector3Int(position.x + block.position.x + offset.x, position.y + block.position.y + offset.y, 0)) != null || !board.PositionInBounds(position + block.position + offset))
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckWithinBoard()
    {
        foreach (PieceBlock block in pieceData.GetBlocks())
        {
            if (position.y + block.position.y >= board.boardHeight)
            {
                return false;
            }
        }

        return true;
    }

    public void ResetPieceData()
    {
        while(dir != 0)
        {
            RotateBlocks(-1);
            dir -= 1;
        }
    }
}