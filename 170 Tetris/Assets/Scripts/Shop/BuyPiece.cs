using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;


public class BuyPiece : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PieceData pieceData;
    public GameManager gameManager;
    public PieceRenderer pieceRenderer;
    public List<PieceData> pieceList; 
    private PieceData piece;
    private Texture2D pieceTexture;


    void Start()
    {
        //chooses a random piece
        PieceData piece = gameManager.RollGatcha();

        pieceTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false); // only needs to be created once
        DrawPiece(); // dont draw every update frame, only when the shop piece visual needs to be updated
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        //adds piece to bagfull
        gameManager.AddPieceToBag(piece);
    }

    // Should be called whenever the piece changes
    public void DrawPiece()
    {
        pieceRenderer.RenderToTexture2D(piece, pieceTexture);

        // Convert the new Texture2D to a Sprite
        Sprite sprite = Sprite.Create(pieceTexture, new Rect(new Vector2(0, 0), new Vector2(256, 256)), new Vector2(0, 0));

        GetComponent<Image>().sprite = sprite; // replace the get component with whatever image you want to overwrite
    }
}