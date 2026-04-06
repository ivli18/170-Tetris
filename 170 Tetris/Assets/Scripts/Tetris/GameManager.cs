using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private bool paused = false;
    private int level = 1;
    private int points = 0;

    //private List<BoardManager> boards;
    //private List<Piece> bagFull;
    //private List<Piece> bagCurrent;
    //private Piece heldPiece = null;
    //private List<Piece> previewPieces;

    private int boardHeight = 20; //base tetris is 20 for the visible height, and 40 is the true height; here it will be higher like 50-70
    private int boardWidth = 10; // hesitant to say this should be changable
    private int spawnDelay = 5; //figure out timing for all of these, floats may be more appropriate; also known as ARE
    private float gravity = 0.016f; // for consistency gravity matches standard tetris gravity notation where 1G = 1 cell per frame
    private int lockDelay = 5; //base tetris is 0.5 seconds
    // NOTE FOR ME: MOVE RESET SAYS YOU MAY ONLY STALL FOR 15 MOVES/ROTATIONS BEFORE LOCKING, MOVING DOWN RESETS THIS
    private const int AUTOSHIFT_DELAY = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
