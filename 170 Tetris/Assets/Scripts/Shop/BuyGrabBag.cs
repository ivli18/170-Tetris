using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;


public class BuyGrabBag : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PieceData pieceData;
    public GameManager gameManager;
    public GameObject pieceRenderer;
    private PieceRenderer pieceRendererScript;
    //public List<PieceData> pieceList; 
    private PieceData piece;
    private Texture2D pieceTexture;
    private Sprite pieceSprite;
    private bool soldOut = false;
    private Sprite bagSprite;
    [SerializeField]
    private Sprite soldText;
    private TextMeshProUGUI textMesh;
    [SerializeField]
    private int price = 40;
    [SerializeField]
    private int freePulls = 0;

    public enum RarityTable { NORMAL, RARE };
    [SerializeField]
    private RarityTable table;


    void Start()
    {
        //get pieceRenderer script
        pieceRendererScript = pieceRenderer.GetComponent<PieceRenderer>();

        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        bagSprite = GetComponent<Image>().sprite;

        pieceTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false); // only needs to be created once

        gameManager.onShopOpen.AddListener(OnShopOpen);
        gameManager.onShopClose.AddListener(OnShopClose);
        gameManager.onCurrencyUpdate.AddListener(OnCurrencyUpdate);

        textMesh.enabled = false;
    }

    private void RollGatcha()
    {
        if (table == RarityTable.NORMAL)
        {
            piece = gameManager.RollGatcha();
        }
        else
        {
            piece = gameManager.RollGatcha(gameManager.rareWeightTable, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        //adds piece to bagfull
        gameManager.AddPieceToBag(piece);
        if (freePulls > 0)
        {
            freePulls--;
        }
        else
        {
            gameManager.AddCurrency(-price);
        }
        SoldOut();
        GetComponent<Image>().sprite = pieceSprite;
        gameManager.audioManager.PlaySoundBuy();
        // show the piece getting added
    }

    public void OnShopOpen()
    {
        RollGatcha();
        if (piece != null)
        {
            soldOut = false;
            DrawPiece();
            OnCurrencyUpdate();
            GetComponent<Image>().sprite = bagSprite;
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
        pieceSprite = Sprite.Create(pieceTexture, new Rect(new Vector2(0, 0), new Vector2(256, 256)), new Vector2(0.5f, 0.5f));

        // GetComponent<Image>().sprite = sprite; // replace the get component with whatever image you want to overwrite
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
            if(freePulls > 0)
            {
                textMesh.color = Color.white;
                GetComponent<Button>().interactable = true;
                textMesh.text = "FREE";
            }
            else if (price < 0)
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