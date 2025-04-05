using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopElement
{
    public Image itemImage;
    public Text itemName;
    public Text itemPrice;

    public ShopElement(Image image, Text name, Text price)
    {
        itemImage = image;
        itemName = name;
        itemPrice = price;
    }
}
