using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuPanel; 
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;   
    [SerializeField] private GameObject actionControlPanel; 

    [Header("Action Buttons")]
    [SerializeField] private GameObject lieGroupObj;         
    [SerializeField] private GameObject confirmButton;       
    [SerializeField] private GameObject challengeGroupObj;   

    [Header("Score & Turn Text References")]
    [SerializeField] private Tile[] allTiles; 
    [SerializeField] private TMP_Text p1ScoreText;        
    [SerializeField] private TMP_Text p2ScoreText;    
    [SerializeField] private TMP_Text turnNotificationText; 

    [Header("Pressure Systems UI")]
    [SerializeField] private TMP_Text timerText;           
    [SerializeField] private TMP_Text ammoStatusText;      

    [Header("Cơ Chế Lộ Đòn (Gợi Ý Tổng)")]
    [SerializeField] private TMP_Text[] rowSumTexts;       
    [SerializeField] private TMP_Text[] colSumTexts;       

    private int[,] realBoard = new int[4, 4];
    private int[,] fakeBoard = new int[4, 4];
    private int[,] ownerBoard = new int[4, 4]; 
    private int[,] tileStatus = new int[4, 4];

    private int p1Score = 0;
    private int p2Score = 0;
    private int currentTurnPlayer = 1; 

    private int p1CatchAmmo = 3;
    private int p2CatchAmmo = 3;

    private Coroutine timerCoroutine;

    private int selectedX, selectedY;
    private bool isWaitingForAction = false;
    private int temporarilySelectedFakeValue = 1; 

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int tileIndex = i * 5 + j; 
                allTiles[tileIndex].SetupCoordinates(i, j);
            }
        }
    }

    private void Start()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        if (actionControlPanel != null) actionControlPanel.SetActive(false);
        if (lieGroupObj != null) lieGroupObj.SetActive(false);
        if (confirmButton != null) confirmButton.SetActive(false);
        if (challengeGroupObj != null) challengeGroupObj.SetActive(false);
    }

    public void PlayGameFromMenu()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        RestartGame();
    }

    public void RestartGame()
    {
        p1Score = 0;
        p2Score = 0;
        p1CatchAmmo = 3;
        p2CatchAmmo = 3;
        currentTurnPlayer = 1;
        isWaitingForAction = false;
        temporarilySelectedFakeValue = 1;

        UpdateScoreUI();
        UpdateTurnUI();
        UpdateAmmoUI(2);
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        if (actionControlPanel != null) actionControlPanel.SetActive(false);
        if (lieGroupObj != null) lieGroupObj.SetActive(false);
        if (confirmButton != null) confirmButton.SetActive(false);
        if (challengeGroupObj != null) challengeGroupObj.SetActive(false);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.text = "";

        System.Array.Clear(realBoard, 0, realBoard.Length);
        System.Array.Clear(fakeBoard, 0, fakeBoard.Length);
        System.Array.Clear(ownerBoard, 0, ownerBoard.Length);
        System.Array.Clear(tileStatus, 0, tileStatus.Length); 

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                realBoard[i, j] = Random.Range(1, 10); 
            }
        }

        CalculateAndDisplaySums();
        UpdateUI();
    }

    private void CalculateAndDisplaySums()
    {
        for (int i = 0; i < 4; i++)
        {
            int rowSum = 0;
            for (int j = 0; j < 4; j++) rowSum += realBoard[i, j];
            if (rowSumTexts != null && i < rowSumTexts.Length && rowSumTexts[i] != null)
                rowSumTexts[i].text = rowSum.ToString();
        }

        for (int j = 0; j < 4; j++)
        {
            int colSum = 0;
            for (int i = 0; i < 4; i++) colSum += realBoard[i, j];
            if (colSumTexts != null && j < colSumTexts.Length && colSumTexts[j] != null)
                colSumTexts[j].text = colSum.ToString();
        }
    }

    public void OnTileSelectedByPlayer(int x, int y)
    {
        if (challengeGroupObj != null && challengeGroupObj.activeSelf) return;
        if (isWaitingForAction && (lieGroupObj != null && !lieGroupObj.activeSelf)) return;
        if (ownerBoard[x, y] != 0) return; 

        if (isWaitingForAction && lieGroupObj != null && lieGroupObj.activeSelf)
        {
            int oldTileIndex = selectedX * 5 + selectedY;
            allTiles[oldTileIndex].SetTileData(realBoard[selectedX, selectedY], fakeBoard[selectedX, selectedY], ownerBoard[selectedX, selectedY], tileStatus[selectedX, selectedY]);
        }

        selectedX = x;
        selectedY = y;
        isWaitingForAction = true;
        temporarilySelectedFakeValue = 1; 

        if (actionControlPanel != null) actionControlPanel.SetActive(true);
        if (lieGroupObj != null) lieGroupObj.SetActive(true);
        if (confirmButton != null) confirmButton.SetActive(true);
        if (challengeGroupObj != null) challengeGroupObj.SetActive(false);
        
        if (ammoStatusText != null) ammoStatusText.text = "";

        int newTileIndex = selectedX * 5 + selectedY;
        allTiles[newTileIndex].SetTileData(realBoard[selectedX, selectedY], temporarilySelectedFakeValue, currentTurnPlayer, 0);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTurnTimer(true)); 
    }

    public void OnSubmitFakeValue(int fakeVal)
    {
        temporarilySelectedFakeValue = fakeVal;
        int tileIndex = selectedX * 5 + selectedY;
        allTiles[tileIndex].SetTileData(realBoard[selectedX, selectedY], fakeVal, currentTurnPlayer, 0);
    }

    private IEnumerator StartTurnTimer(bool isSelectionPhase)
    {
        int timeLeft = 10;
        while (timeLeft > 0)
        {
            if (timerText != null) timerText.text = timeLeft.ToString(); 
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
        }

        if (timerText != null) timerText.text = "0";

        if (isSelectionPhase) OnConfirmFakeValue(); 
        else OnResolveChallenge(false);
    }

    public void OnConfirmFakeValue()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        fakeBoard[selectedX, selectedY] = temporarilySelectedFakeValue;
        ownerBoard[selectedX, selectedY] = currentTurnPlayer; 
        UpdateUI();

        int opponent = currentTurnPlayer == 1 ? 2 : 1;
        UpdateAmmoUI(opponent);

        if (lieGroupObj != null) lieGroupObj.SetActive(false);
        if (confirmButton != null) confirmButton.SetActive(false);
        if (challengeGroupObj != null) challengeGroupObj.SetActive(true);

        timerCoroutine = StartCoroutine(StartTurnTimer(false));
    }

    public void OnResolveChallenge(bool didChallenge)
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        
        if (actionControlPanel != null) actionControlPanel.SetActive(false);
        if (lieGroupObj != null) lieGroupObj.SetActive(false);
        if (confirmButton != null) confirmButton.SetActive(false);
        if (challengeGroupObj != null) challengeGroupObj.SetActive(false);

        int realVal = realBoard[selectedX, selectedY];
        int fakeVal = fakeBoard[selectedX, selectedY];
        int opponent = currentTurnPlayer == 1 ? 2 : 1;

        int opponentAmmo = opponent == 1 ? p1CatchAmmo : p2CatchAmmo;
        if (didChallenge && opponentAmmo <= 0) didChallenge = false;

        if (didChallenge)
        {
            if (opponent == 1) p1CatchAmmo--; else p2CatchAmmo--;

            if (realVal != fakeVal) 
            {
                tileStatus[selectedX, selectedY] = 2; 

                if (currentTurnPlayer == 1) { p1Score -= fakeVal; p2Score += realVal; }
                else { p2Score -= fakeVal; p1Score += realVal; }
                ownerBoard[selectedX, selectedY] = opponent; 
            }
            else 
            {
                tileStatus[selectedX, selectedY] = 3;

                int penalty = fakeVal + 10; 
                if (currentTurnPlayer == 1) { p1Score += (realVal * 2); p2Score -= penalty; }
                else { p2Score += (realVal * 2); p1Score -= penalty; }
            }
        }
        else 
        {
            tileStatus[selectedX, selectedY] = 1;

            if (currentTurnPlayer == 1) p1Score += fakeVal;
            else p2Score += fakeVal;
        }

        UpdateScoreUI();
        UpdateUI();

        int tileIndex = selectedX * 5 + selectedY;
        if (!didChallenge) allTiles[tileIndex].PlayPassAnimation();
        else if (realVal != fakeVal) allTiles[tileIndex].PlayCatchSuccessAnimation();
        else allTiles[tileIndex].PlayCatchFailAnimation();

        if (CheckGameOver()) 
        {
            TriggerGameOver();
        }
        else 
        {
            StartCoroutine(EndTurnSequence(opponent));
        }
    }

    private IEnumerator EndTurnSequence(int nextPlayer)
    {
        yield return new WaitForSeconds(1.5f);
        
        if (timerText != null) timerText.text = "";
        
        currentTurnPlayer = nextPlayer;
        isWaitingForAction = false; 

        UpdateTurnUI();
        UpdateUI();
    }

    private bool CheckGameOver()
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                if (ownerBoard[i, j] == 0) return false;
        return true;
    }

    private void TriggerGameOver()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.text = "";
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        string winnerText = p1Score > p2Score ? "PLAYER 1 WIN!" : (p2Score > p1Score ? "PLAYER 2 WIN!" : "HOA NHAU!");
        turnNotificationText.text = winnerText;
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "Player 1: " + p1Score.ToString();
        if (p2ScoreText != null) p2ScoreText.text = "Player 2: " + p2Score.ToString();
    }

    private void UpdateTurnUI()
    {
        if (turnNotificationText != null && !CheckGameOver())
        {
            turnNotificationText.text = $"LUOT CUA PLAYER {currentTurnPlayer}";
        }
    }

    private void UpdateAmmoUI(int targetOpponent)
    {
        if (ammoStatusText == null) return;
        int ammoLeft = targetOpponent == 1 ? p1CatchAmmo : p2CatchAmmo;
        if (ammoLeft > 0)
        {
            ammoStatusText.text = $"Player {targetOpponent} has {ammoLeft} catch";
            ammoStatusText.color = Color.green;
        }
        else
        {
            ammoStatusText.text = $"Player {targetOpponent} without 0 catch";
            ammoStatusText.color = Color.red; 
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int tileIndex = i * 5 + j;
                allTiles[tileIndex].SetTileData(realBoard[i, j], fakeBoard[i, j], ownerBoard[i, j], tileStatus[i, j]);
            }
        }
    }
}