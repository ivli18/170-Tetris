using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool paused = false;
    private int level = 1;
    private int points = 0;

    //private List<BoardManager> boards;
    private int activeBoard = 0;
    private List<PieceData> bagFull;
    private List<PieceData> bagCurrent;
    private PieceData heldPiece = null;
    private List<PieceData> previewPieces;

    private int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    private int boardWidth = 10; // hesitant to say this should be changable
    private int spawnDelay = 5; //figure out timing for all of these, floats may be more appropriate; also known as ARE
    private float gravity = 0.016f; // for consistency gravity matches standard tetris gravity notation where 1G = 1 cell per frame (60fps)
    private int lockDelay = 5; //base tetris is 0.5 seconds
    private float timeSinceStep = 0;
    // NOTE FOR ME: MOVE RESET SAYS YOU MAY ONLY STALL FOR 15 MOVES/ROTATIONS BEFORE LOCKING, MOVING DOWN RESETS THIS
    private int stallMoves = 0;
    private static readonly int STALL_MOVES_MAX = 15;
    private static readonly int AUTOSHIFT_DELAY = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if piece is on the board
        // check block overlap below
        //  if so, run based off lock_delay
        // else
        //  check input
        //  apply step if gravity, ensure multiple steps can happen per frame
    }
}

public class Piece
{
    private int dir = 0;

    private void GetInputs()
    {

    }

    private void Step()
    {

    }

    private void Rotate(int dir)
    {
        
    }

    private void Move(Vector2Int offset)
    {

    }

    private void CheckBlockOverlap(Vector2Int offest)
    {

    }

    private void HardDrop()
    {

    }
}