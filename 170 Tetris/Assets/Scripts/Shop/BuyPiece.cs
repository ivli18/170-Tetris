using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;


public class BuyPiece : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PieceData pieceData;
    public GameManager gameManager;
    public GameObject pieceRenderer;
    private PieceRenderer pieceRendererScript;
    //public List<PieceData> pieceList; 
    private PieceData piece;
    private Texture2D pieceTexture;
    private bool soldOut = false;
    [SerializeField]
    private Sprite soldText;
    private TextMeshProUGUI textMesh;


    void Start()
    {
        //get pieceRenderer script
        pieceRendererScript = pieceRenderer.GetComponent<PieceRenderer>();

        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        //chooses a random piece
        piece = gameManager.RollGatcha();

        pieceTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false); // only needs to be created once
        DrawPiece(); // dont draw every update frame, only when the shop piece visual needs to be updated

        gameManager.onShopOpen.AddListener(OnShopOpen);
        gameManager.onShopClose.AddListener(OnShopClose);
        gameManager.onCurrencyUpdate.AddListener(OnCurrencyUpdate);

        textMesh.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        //adds piece to bagfull
        gameManager.AddPieceToBag(piece);
        gameManager.AddCurrency(gameManager.priceByRarity[(int)piece.GetRarity()] * -1);
        SoldOut();
    }

    public void OnShopOpen()
    {
        piece = gameManager.RollGatcha();
        if (piece != null)
        {
            soldOut = false;
            DrawPiece();
            OnCurrencyUpdate();
        }
        else
        {
            SoldOut();
        }
    }

    public void OnShopClose()
    {
        if (!soldOut)
        {
            gameManager.AddToPullList(piece);
        }
    }

    // Should be called whenever the piece changes
    public void DrawPiece()
    {
        GetComponent<Button>().interactable = true;
        pieceRendererScript.RenderToTexture2D(piece, pieceTexture);

        // Convert the new Texture2D to a Sprite
        Sprite sprite = Sprite.Create(pieceTexture, new Rect(new Vector2(0, 0), new Vector2(256, 256)), new Vector2(0.5f, 0.5f));

        GetComponent<Image>().sprite = sprite; // replace the get component with whatever image you want to overwrite
        int price = gameManager.priceByRarity[(int) piece.GetRarity()];
        if(price > 0)
        {
            textMesh.text = price.ToString();
            textMesh.color = Color.white;
        }
        else
        {
            price *= -1;
            textMesh.text = "+" + price.ToString();
            textMesh.color = Color.green;
        }
        textMesh.enabled = true;
    }

    public void SoldOut()
    {
        GetComponent<Button>().interactable = false;
        GetComponent<Image>().sprite = soldText;
        textMesh.enabled = false;
        soldOut = true;
    }

    public void OnCurrencyUpdate()
    {
        if (!soldOut)
        {
            int price = gameManager.priceByRarity[(int)piece.GetRarity()];
            if (price < 0)
            {

            }
            else if (gameManager.points < price)
            {
                textMesh.color = Color.red;
                GetComponent<Button>().interactable = false;
            }
            else
            {
                textMesh.color = Color.white;
                GetComponent<Button>().interactable = true;
            }
        }
    }
}