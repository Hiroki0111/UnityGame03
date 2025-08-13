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
        humanLeftText.text = "�����؂����l��: " + ResultData.humanLeft;
        playerConvertedText.text = "���Ȃ����]���r�ɂ�����: " + ResultData.playerConverted;
        cpuConvertedText.text = "CPU���]���r�ɂ�����: " + ResultData.cpuConverted;

        backgroundImage.sprite = ResultData.isWin ? winSprite : loseSprite;
    }
}
