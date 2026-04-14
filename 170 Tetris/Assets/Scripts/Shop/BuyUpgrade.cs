using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Events;


public class BuyUpgrade : MonoBehaviour
{
    public GameManager gameManager;
    private bool soldOut = false;
    private Sprite baseSprite;
    [SerializeField]
    private Sprite soldText;
    private TextMeshProUGUI textMesh;
    [SerializeField]
    private int[] costPerPurchase = { 50, 100, 150, 200 };
    protected int timesPurchased = 0;
    public UnityEvent OnPurchase;


    void Start()
    {
        baseSprite = GetComponent<Image>().sprite;
        textMesh = transform.Find("Pricetext").GetComponent<TextMeshProUGUI>();

        gameManager.onShopOpen.AddListener(OnShopOpen);
        gameManager.onCurrencyUpdate.AddListener(OnCurrencyUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        OnPurchase.Invoke();
        gameManager.AddCurrency(-costPerPurchase[timesPurchased]);
        SoldOut();
        timesPurchased += 1;
    }

    public virtual void OnShopOpen()
    {
        if (timesPurchased < costPerPurchase.Length)
        {
            GetComponent<Image>().sprite = baseSprite;
            textMesh.enabled = true;
            soldOut = false;
            textMesh.text = costPerPurchase[timesPurchased].ToString();
            OnCurrencyUpdate();
        }
        else
        {
            SoldOut();
        }
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
            int price = costPerPurchase[timesPurchased];

            if (gameManager.points < price)
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