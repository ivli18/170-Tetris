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
        //todo: add gacha weights, choose a new piece each shop visit, choose a new piece if selected piece in bagfull
        pieceList = gameManager.bagFull;
        int index = Random.Range(0, pieceList.Count);
        PieceData piece = pieceList[index];
        print(pieceList.Count);
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