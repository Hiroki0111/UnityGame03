using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    public Text humanLeftText;
    public Text playerConvertedText;
    public Text cpuConvertedText;

    public Image backgroundImage;
    public Sprite winSprite;
    public Sprite loseSprite;

    void Start()
    {
        humanLeftText.text = "“¦‚°Ø‚Á‚½lŠÔ: " + ResultData.humanLeft;
        playerConvertedText.text = "‚ ‚È‚½‚ªƒ]ƒ“ƒr‚É‚µ‚½”: " + ResultData.playerConverted;
        cpuConvertedText.text = "CPU‚ªƒ]ƒ“ƒr‚É‚µ‚½”: " + ResultData.cpuConverted;

        backgroundImage.sprite = ResultData.isWin ? winSprite : loseSprite;
    }
}
