using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// This class represents a shop element in the game.
/// <summary>
public class ShopElement
{
    public Image ItemImage { get; set; }
    public Text ItemName { get; set; }
    public Text ItemPrice { get; set; }

    public ShopElement(Image image, Text name, Text price)
    {
        ItemImage = image;
        ItemName = name;
        ItemPrice = price;
    }
}
