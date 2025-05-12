using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ShopElementTests
{
    [Test]
    public void Constructor_AssignsFieldsCorrectly()
    {
        // Külön GameObject minden UI elemhez
        var imageGO = new GameObject("ImageGO");
        var textNameGO = new GameObject("TextNameGO");
        var textPriceGO = new GameObject("TextPriceGO");

        var image = imageGO.AddComponent<Image>();
        var nameText = textNameGO.AddComponent<Text>();
        var priceText = textPriceGO.AddComponent<Text>();

        nameText.text = "Test Item";
        priceText.text = "99";

        var shopElement = new ShopElement(image, nameText, priceText);

        Assert.AreEqual(image, shopElement.itemImage);
        Assert.AreEqual(nameText, shopElement.itemName);
        Assert.AreEqual(priceText, shopElement.itemPrice);
        Assert.AreEqual("Test Item", shopElement.itemName.text);
        Assert.AreEqual("99", shopElement.itemPrice.text);
    }
}
