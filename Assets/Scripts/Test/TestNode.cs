using UnityEngine.UI;
using FT;
using UnityEngine;

public class TestNode : FTNodeBase
{
    public Text text;
    public Image image;
    public CanvasGroup canvasGroup;
    public bool isMultSize;

    public void Refresh(string word)
    {
        text.text = word;
        if (isMultSize)
        {
            var i = int.Parse(word);
            SizeDelta = new Vector2(Width, 100 + i * 10);
        }
    }
}
