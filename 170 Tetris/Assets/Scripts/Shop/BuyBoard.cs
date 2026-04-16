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
        if (gameManager.GetBoardCount() >= 3)
        {
            SoldOut();
        }
        else
        {
            base.OnShopOpen();
        }
    }
}