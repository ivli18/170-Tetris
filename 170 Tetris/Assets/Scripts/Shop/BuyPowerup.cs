
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Events;


public class BuyPowerup : MonoBehaviour
{
    public GameManager gameManager;
    private bool soldOut = false;
    private Sprite baseSprite;
    [SerializeField]
    private Sprite soldText;
    private TextMeshProUGUI textMesh;
    [SerializeField]
    private int[] prices;
    [SerializeField]
    private Sprite[] powerupSprites;
    public UnityEvent OnPurchase;
    private TextMeshProUGUI tooltipText;
    private int powerup;


    void Start()
    {
        baseSprite = GetComponent<Image>().sprite;
        textMesh = transform.Find("Pricetext").GetComponent<TextMeshProUGUI>();
        tooltipText = transform.Find("Tooltip").GetComponentInChildren<TextMeshProUGUI>();

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
        gameManager.AddCurrency(-prices[powerup]);
        switch(powerup)
        {
            case 0:
                gameManager.ActivatePowerup(Powerup.SLOW, 0);
                break;
            case 1:
                gameManager.ActivatePowerup(Powerup.MULT, 0);
                break;
            case 2:
                gameManager.ActivatePowerup(Powerup.GRAVITY, 0);
                break;
        }
        SoldOut();
        gameManager.audioManager.PlaySoundBuy();
    }

    public virtual void OnShopOpen()
    {
        powerup = Random.Range(0, 3);
        GetComponent<Image>().sprite = powerupSprites[powerup];
        textMesh.enabled = true;
        soldOut = false;
        textMesh.text = (prices[powerup]).ToString();
        OnCurrencyUpdate();
        switch (powerup)
        {
            case 0:
                tooltipText.text = "Reduce the gravity for this level.";
                break;
            case 1:
                tooltipText.text = "Line clears grant double points this level.";
                break;
            case 2:
                tooltipText.text = "Drop all blocks on the active board, granting 5 points per line cleared.";
                break;
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
            if (gameManager.points < prices[powerup])
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