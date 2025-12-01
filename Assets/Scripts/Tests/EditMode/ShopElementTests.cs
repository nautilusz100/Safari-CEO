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

        Assert.AreEqual(image, shopElement.ItemImage);
        Assert.AreEqual(nameText, shopElement.ItemName);
        Assert.AreEqual(priceText, shopElement.ItemPrice);
        Assert.AreEqual("Test Item", shopElement.ItemName.text);
        Assert.AreEqual("99", shopElement.ItemPrice.text);
    }
}
