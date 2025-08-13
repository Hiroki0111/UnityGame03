using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro �p

public class ResultSceneManager : MonoBehaviour
{
    [Header("�l���\���p�e�L�X�g�iTextMeshProUGUI�j")]
    public TextMeshProUGUI humanLeftText;
    public TextMeshProUGUI playerConvertedText;
    public TextMeshProUGUI cpuConvertedText;

    [Header("�w�i�摜��\������UI�iImage�R���|�[�l���g�j")]
    public Image backgroundImage;

    [Header("�������̔w�i�X�v���C�g")]
    public Sprite winSprite;

    [Header("�s�k���̔w�i�X�v���C�g")]
    public Sprite loseSprite;

    [Header("�^�C�g���ɖ߂�{�^��")]
    public Button backToTitleButton;

    void Start()
    {
        // Null �`�F�b�N
        if (humanLeftText == null) Debug.LogError("humanLeftText ���ݒ肳��Ă��܂���");
        if (playerConvertedText == null) Debug.LogError("playerConvertedText ���ݒ肳��Ă��܂���");
        if (cpuConvertedText == null) Debug.LogError("cpuConvertedText ���ݒ肳��Ă��܂���");
        if (backgroundImage == null) Debug.LogError("backgroundImage ���ݒ肳��Ă��܂���");
        if (backToTitleButton == null) Debug.LogWarning("backToTitleButton ���ݒ肳��Ă��܂���");

        // ResultData ����l���擾���ĕ\��
        if (humanLeftText != null)
            humanLeftText.text = "�c��Z�l: " + ResultData.humanLeft;
        if (playerConvertedText != null)
            playerConvertedText.text = "�]���r�̐�: " + ResultData.playerConverted;
        if (cpuConvertedText != null)
            cpuConvertedText.text = "���]���r�̐�: " + ResultData.cpuConverted;

        // ���s�ɂ���Ĕw�i�������ւ�
        if (backgroundImage != null)
            backgroundImage.sprite = ResultData.isWin ? winSprite : loseSprite;

        // �{�^���ɃN���b�N�C�x���g��ǉ�
        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.AddListener(ReturnToTitle);
        }
    }

    // �^�C�g���V�[���ɖ߂�
    void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene"); // �^�C�g���V�[�������m�F
    }
}
