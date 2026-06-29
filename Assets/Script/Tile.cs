using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private Image backgroundImage;

    [Header("Visual Configuration")]
    [SerializeField] private Color[] playerColors;

    private int gridX;
    private int gridY;
    private int realValue = 0;
    private int fakeValue = 0;
    private int ownerId = 0;
    private int tileStatus = 0;
    private int specialType = 0;        

    private GridManager gridManager;
    private Tweener scaleTween;

    private void Awake()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        InitializeColors();

        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(OnTileClicked);
    }

    private void InitializeColors()
    {
        if (playerColors == null || playerColors.Length < 3)
        {
            playerColors = new Color[3]
            {
                new Color(0.80f, 0.80f, 0.80f),
                new Color(0.23f, 0.48f, 0.85f),
                new Color(0.85f, 0.23f, 0.23f)
            };
        }
    }

    public void SetupCoordinates(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public void SetTileData(int realVal, int fakeVal, int owner, int status, int special)
    {
        realValue = realVal;
        fakeValue = fakeVal;
        ownerId = owner;
        tileStatus = status;
        specialType = special;

        Color baseColor = playerColors[ownerId];
        string specialTag = "";

        if (specialType == 1)
            specialTag = "\n<size=40%><color=#FF5555>[x2]</color></size>";
        else if (specialType == 2)
            specialTag = "\n<size=40%><color=#FF00FF>[ALL-IN]</color></size>";

        if (status == 0) 
        {
            numberText.text = fakeVal == 0 ? "" : fakeVal.ToString();
            if (specialType == 1 || specialType == 2)
            {
                numberText.text += specialTag;
                numberText.color = new Color(1f, 0.7f, 0f); 
                backgroundImage.color = Color.Lerp(baseColor, Color.black, 0.2f);
            }
            else
            {
                numberText.color = Color.white;
                backgroundImage.color = baseColor;
            }
        }
        else if (status == 1)  
        {
            numberText.text = realVal.ToString() + specialTag;
            numberText.color = Color.white;
            backgroundImage.color = baseColor;
        }
        else if (status == 2)  
        {
            numberText.text = realVal.ToString() + specialTag;
            numberText.color = Color.yellow;
            backgroundImage.color = Color.Lerp(baseColor, Color.black, 0.4f);
        }
        else if (status == 3)  
        {
            numberText.text = realVal.ToString() + specialTag;
            numberText.color = Color.yellow;
            backgroundImage.color = Color.Lerp(baseColor, Color.yellow, 0.5f);
        }
    }

    public void PlayPassAnimation()
    {
        KillTween();
        scaleTween = transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0), 0.3f, 2, 1);
    }

    public void PlayCatchSuccessAnimation()
    {
        KillTween();
        scaleTween = transform.DOShakeRotation(0.5f, new Vector3(0, 0, 30f), 15, 90f);
    }

    public void PlayCatchFailAnimation()
    {
        KillTween();
        scaleTween = transform.DOPunchScale(new Vector3(0.4f, 0.4f, 0), 0.5f, 5, 1);
    }

    private void OnTileClicked()
    {
        if (ownerId != 0 || tileStatus != 0) return;
        if (gridManager != null)
        {
            gridManager.OnTileSelectedByPlayer(gridX, gridY);
        }
    }

    public int GetRealValue() => realValue;
    public int GetFakeValue() => fakeValue;
    public int GetOwnerId() => ownerId;

    private void KillTween()
    {
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();
    }

    private void OnDestroy() => KillTween();
}