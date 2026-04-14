using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Events;


public class BuyBoard : BuyUpgrade
{
    public override void OnShopOpen()
    {
        timesPurchased = gameManager.GetBoardCount() - 1;

        base.OnShopOpen();
    }
}