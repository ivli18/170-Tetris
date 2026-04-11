using UnityEngine;
using System.Collections.Generic;


public class BuyPiece : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PieceData pieceData;
    public GameManager gameManager;
    private PieceViewer pieceViewer;
    public List<PieceData> pieceList; 
    private PieceData piece;


    void Start()
    {
        //chooses a random piece
        PieceData piece = gameManager.RollGatcha();
    }

    // Update is called once per frame
    void Update()
    {
        //ts harder than i thought
        //pieceViewer.DrawPiece(piece, currentPos);
    }

    public void OnClick()
    {
        //adds piece to bagfull
        gameManager.AddPieceToBag(piece);
    }
}