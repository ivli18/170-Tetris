using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Audio.ProcessorInstance;
using UnityEngine.Tilemaps;
using TMPro;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    public List<PieceData> allPieces;
    private List<PieceData>[] piecePullTables = {new List<PieceData>(), new List<PieceData>(), new List<PieceData>(), new List<PieceData>(), new List<PieceData>(), new List<PieceData>() };
    [SerializeField]
    public int[] baseWeightTable = { 100, 80, 60, 40, 20, 10 };
    public int[] rareWeightTable = { 0, 0, 100, 60, 40, 20 };

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
    private InputAction actionSwitchBoardLeft;
    private InputAction actionSwitchBoardRight;
    private InputAction actionSwitchBoard1;
    private InputAction actionSwitchBoard2;
    private InputAction actionSwitchBoard3;
    private InputAction actionSwitchBoard4;
    private InputAction actionsPause;

    public Tilemap heldPieceUI;
    public TextMeshProUGUI currencyTextValue;
    public Tilemap previewPieceUI;
    
    public GameObject PauseScreen;
    private SpriteRenderer PauseSprite;
    public GameObject ShopUI;
    private Animator shopAnim;
    
    public AudioManager audioManager;
    public GameObject boardPrefab;

    private void Awake()
    {
        SetupPullTable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Default is the existing input system, yet to add toggles
        // InputSystem.actions.bindingMask = InputBinding.MaskByGroup("Tetris WASD");
        // InputSystem.actions.bindingMask = InputBinding.MaskByGroup("Tetris Arrows");
        actionShiftLeft = InputSystem.actions.FindAction("ShiftLeft");
        actionShiftRight = InputSystem.actions.FindAction("ShiftRight");
        actionHardDrop = InputSystem.actions.FindAction("HardDrop");
        actionSoftDrop = InputSystem.actions.FindAction("SoftDrop");
        actionRotateClockwise = InputSystem.actions.FindAction("RotateClockwise");
        actionRotateCounterclockwise = InputSystem.actions.FindAction("RotateCounterclockwise");
        actionHold = InputSystem.actions.FindAction("Hold");
        actionsPause = InputSystem.actions.FindAction("Pause");
        actionSwitchBoardLeft = InputSystem.actions.FindAction("SwitchBoardLeft");
        actionSwitchBoardRight = InputSystem.actions.FindAction("SwitchBoardRight");
        actionSwitchBoard1 = InputSystem.actions.FindAction("SwitchBoard1");
        actionSwitchBoard2 = InputSystem.actions.FindAction("SwitchBoard2");
        actionSwitchBoard3 = InputSystem.actions.FindAction("SwitchBoard3");
        actionSwitchBoard4 = InputSystem.actions.FindAction("SwitchBoard4");

        currencyTextValue.SetText(points.ToString());

        ShufflePieces();
        ShufflePreviewPieces();
        CalculateGravity();
        AdjustBoardPositions();
        boards[0].SetActive();

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
        else
        {
            if (actionsPause.WasPressedThisFrame())
            {
                CloseShop();
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
            ClearSwapGhosts();

            DebugButtons();

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

            InputSwitchBoard();
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
                    if(activePiece.lowestRow > activePiece.position.y)
                    {
                        stallMoves = 0;
                        activePiece.lowestRow = activePiece.position.y;
                    }
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
            DrawSwapGhosts();
        }
    }

    private void InputSwitchBoard()
    {
        if(actionSwitchBoardLeft.WasPressedThisFrame())
        {
            int boardToSwitch = activeBoard - 1;
            if(boardToSwitch < 0) { return; }//{ boardToSwitch = boards.Count - 1; }
            if(activePiece.CheckForEmptySpace(new Vector2Int(0, 0), boards[boardToSwitch]))
            {
                SetActiveBoard(boardToSwitch);
            }
        }
        else if (actionSwitchBoardRight.WasPressedThisFrame())
        {
            int boardToSwitch = activeBoard + 1;
            if (boardToSwitch >= boards.Count) { return; }//{ boardToSwitch = 0; }
            if (activePiece.CheckForEmptySpace(new Vector2Int(0, 0), boards[boardToSwitch]))
            {
                SetActiveBoard(boardToSwitch);
            }
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

        activePiece.position = new Vector2Int(4, boardHeight - 1) - activePiece.pieceData.GetCenter();
        ghostPiece.position = activePiece.position;

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

    private void ShufflePreviewPieces()
    {
        previewPieces.Clear();
        for (int i = 0; i < previewPieceCount; i++)
        {
            previewPieces.Add(bagCurrent[0]);
            bagCurrent.RemoveAt(0);
            if (bagCurrent.Count <= 0)
            {
                ShufflePieces();
            }
        }
        ClearPreviewPieces();
        DrawPreviewPieces();
    }

    private void PlacePiece()
    {
        boards[activeBoard].DrawPiece(activePiece);
        ClearSwapGhosts();
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
            //print("LINE CLEAR: " + (points - pointsToAdd) + " + " + pointsToAdd + " = " + points);
            currencyTextValue.SetText(points.ToString());
            audioManager.PlaySoundClear();
            if (lineClears >= levelRequirement)
            {
                lineClears = 0;
                LevelClear();
            }
        }
        else
        {
            if(combo > 0)
            {
                points += (combo * 5);
                //print("COMBO: " + (points - (combo * 5)) + " + " + (combo * 5) + " = " + points);
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
        piecePullTables[(int) piece.GetRarity()].Remove(piece);
        ShufflePieces();
        ShufflePreviewPieces();
    }

    public void RemovePieceFromBag(PieceData piece)
    {
        piecePullTables[(int) piece.GetRarity()].Add(piece);
        ShufflePieces();
        ShufflePreviewPieces();
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
        CalculateGravity();
        OpenShop();
    }

    private void OpenShop()
    {
        shopOpen = true;
        shopAnim.Play("shopOpen");
    }

    public void CloseShop()
    {
        shopOpen = false;
        shopAnim.Play("shopClose");
    }

    private void Pause()
    {
        paused = true;
        if(!shopOpen)
        {
            PauseSprite.enabled = true;
        }
    }

    private void Unpause()
    {
        paused = false;
        PauseSprite.enabled = false;
    }

    // Calculate the gravity based on the current level. Uses official tetris guideline values.
    private void CalculateGravity()
    {
        gravity = Math.Min((1.0f / (float) Math.Pow(0.8f - ((level - 1) * 0.007f), level - 1)) / 60.0f, (float) boardHeight);
    }

    // Set up the gatcha rarity tables
    public void SetupPullTable()
    {
        foreach(PieceData piece in allPieces)
        {
            if (!bagFull.Contains(piece))
            {
                piecePullTables[(int)piece.GetRarity()].Add(piece);
            }
        }
    }

    // Current implementation means multiples of each piece could appear in the shop. There is a fairly easy fix if we want it.
    // Roll a piece on the gatcha tables
    public PieceData RollGatcha(int[] weightTable, bool repeats)
    {
        PieceData piece = null;
        while (piece == null)
        {
            // Choose a table to pull from based on the weightTable weights
            int weightSum = 0;
            for (int i = 0; i < weightTable.Length; i++)
            {
                if (piecePullTables[i].Count != 0)
                {
                    weightSum += weightTable[i];
                }
            }

            if(weightSum == 0)
            {
                // add fallback for no tables having a valid roll
            
            }
            int rnd = UnityEngine.Random.Range(0, weightSum);
            int tableToPull = 0;
            for (int i = 0; i < weightTable.Length; i++)
            {
                if (piecePullTables[i].Count != 0)
                {
                    if (rnd < weightTable[i])
                    {
                        tableToPull = i;
                        break;
                    }
                    rnd -= weightTable[i];
                }
            }

            int randPiece = UnityEngine.Random.Range(0, piecePullTables[tableToPull].Count);
            piece = piecePullTables[tableToPull][randPiece];
        }

        return piece;
    }

    // Roll a piece on the gatcha tables using the base weight table
    public PieceData RollGatcha()
    {
        return RollGatcha(baseWeightTable, false);
    }

    // Add a new board to the game
    public void AddBoard()
    {
        GameObject board = Instantiate(boardPrefab);
        BoardManager boardManager =  board.GetComponent<BoardManager>();
        boardManager.boardWidth = boardWidth;
        boardManager.boardHeight = boardHeight;
        boardManager.SetupBoard();
        boards.Add(boardManager);
        AdjustBoardPositions();
    }

    // Resets the board positions to be centered on screen, necessary whenever a board is added/removed
    private void AdjustBoardPositions()
    {
        const int OFFSET = 6;
        float offset_math = 0;
        for (int i = 0; i < boards.Count; i++)
        {
            offset_math = ((float) i - ((boards.Count - 1.0f) / 2.0f)) * (OFFSET * 2.0f + boardWidth - 10.0f);
            boards[i].GetComponent<Transform>().position = new Vector3(offset_math - (boardWidth / 2.0f), -boardHeight / 2.0f, 0);
        }

        previewPieceUI.GetComponent<Transform>().position = new Vector3(7.5f + offset_math, -4, 0);
    }

    // Increase/Reduce board size (reducing size currently does not remove out of bounds blocks)
    public void AddBoardSize(int x, int y)
    {
        boardWidth += x;
        boardHeight += y;

        UpdateBoardSizes();
    }

    // Updates the size of every board to match gameManager's width = height
    public void UpdateBoardSizes()
    {
        foreach(BoardManager board in boards)
        {
            board.boardWidth = boardWidth;
            board.boardHeight = boardHeight;
            board.SetupBoard();
        }
        AdjustBoardPositions();
    }

    // Sets a new board to active; does not check to see if the swap is valid
    public void SetActiveBoard(int boardNum)
    {
        int prevActiveBoard = activeBoard;
        activeBoard = boardNum;
        if(activePiece != null)
        {
            activePiece.board = boards[activeBoard];
            ghostPiece.board = boards[activeBoard];
        }
        boards[activeBoard].SetActive();
        boards[prevActiveBoard].SetInactive();
    }

    // Draws ghost pieces on inactive boards
    private void DrawSwapGhosts()
    {
        for(int i = 0; i < boards.Count; i++)
        {
            if(i != activeBoard)
            {
                boards[i].DrawSwapGhostPiece(activePiece);
            }
        }
    }

    // Clears ghost pieces on inactive boards
    private void ClearSwapGhosts()
    {
        for (int i = 0; i < boards.Count; i++)
        {
            if (i != activeBoard)
            {
                boards[i].ClearSwapGhostPiece(activePiece);
            }
        }
    }

    // Used only for debug
    private void DebugButtons()
    {
        if(actionSwitchBoard1.WasPressedThisFrame())
        {
            AddBoard();
        }
        if (actionSwitchBoard2.WasPressedThisFrame())
        {
            AddBoardSize(1, 0);
        }
        if (actionSwitchBoard3.WasPressedThisFrame())
        {
            AddBoardSize(0, 1);
        }
    }
}

public class Piece
{
    private int dir = 0;
    public Vector2Int position = new Vector2Int(0, 0);
    public PieceData pieceData;
    public BoardManager board;
    public int lowestRow = 100;

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

    public bool CheckForEmptySpace(Vector2Int offset, BoardManager board)
    {
        foreach (PieceBlock block in pieceData.GetBlocks())
        {
            if (board.blocks.GetTile(new Vector3Int(position.x + block.position.x + offset.x, position.y + block.position.y + offset.y, 0)) != null || !board.PositionInBounds(position + block.position + offset))
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