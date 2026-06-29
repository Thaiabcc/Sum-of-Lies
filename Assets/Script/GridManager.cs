using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Unity.Netcode; 

public class GridManager : NetworkBehaviour 
{
    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuPanel;  
    [SerializeField] private GameObject lobbyPanel;      
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;   
    [SerializeField] private GameObject actionControlPanel; 

    [Header("Lobby UI")]
    [SerializeField] private GameObject startGameButton; 
    [SerializeField] private TMP_Text lobbyStatusText;   

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

    [Header("Hint")]
    [SerializeField] private TMP_Text[] rowSumTexts;       
    [SerializeField] private TMP_Text[] colSumTexts;       

    private int[,] realBoard = new int[4, 4];
    private int[,] fakeBoard = new int[4, 4];
    private int[,] ownerBoard = new int[4, 4]; 
    private int[,] tileStatus = new int[4, 4];
    private int[,] specialType = new int[4, 4];

    private int p1Score = 0;
    private int p2Score = 0;
    private int currentTurnPlayer = 1; 

    private int p1CatchAmmo = 3;
    private int p2CatchAmmo = 3;

    private Coroutine timerCoroutine;

    private int selectedX, selectedY;
    private bool isWaitingForAction = false;
    private int temporarilySelectedFakeValue = 1; 

    private bool isGameOver = false;
    private bool hasLockedLocalTile = false; 
    private bool hasConfirmedThisTurn = false; 
    private bool hasResolvedThisTurn = false;  

    private int emptyTilesCount = 16; 

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
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (actionControlPanel != null) actionControlPanel.SetActive(false);
    }

    public void StartHostMode()
    {
        NetworkManager.Singleton.StartHost();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (startGameButton != null) startGameButton.SetActive(true);
        if (lobbyStatusText != null) lobbyStatusText.text = "Waiting for Player 2...";
        
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
                if (lobbyStatusText != null) lobbyStatusText.text = "<color=yellow>Player 2 has joined! Click START to play!</color>";
        };
    }

    public void StartClientMode()
    {
        NetworkManager.Singleton.StartClient();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (startGameButton != null) startGameButton.SetActive(false);
        if (lobbyStatusText != null) lobbyStatusText.text = "Joined room. Waiting for host...";
    }

    public void PlayGameFromLobby()
    {
        if (!IsServer) return;

        int[] flatRealBoard = new int[16];
        int[] flatSpecial = new int[16];
        List<int> validSpecialIndices = new List<int>();

        for (int i = 0; i < 16; i++)
        {
            flatRealBoard[i] = Random.Range(1, 10);
            flatSpecial[i] = 0;
            if (flatRealBoard[i] >= 3)
                validSpecialIndices.Add(i);
        }

        for (int k = 0; k < 3; k++)
        {
            if (validSpecialIndices.Count == 0) break;
            int randIdx = Random.Range(0, validSpecialIndices.Count);
            int boardIdx = validSpecialIndices[randIdx];

            if (k < 2)
                flatSpecial[boardIdx] = 1; 
            else
            {
                flatSpecial[boardIdx] = 2;
                if (Random.value < 0.65f)
                    flatRealBoard[boardIdx] = Random.Range(5, 8);
                else
                    flatRealBoard[boardIdx] = Random.Range(1, 5);
            }
            validSpecialIndices.RemoveAt(randIdx);
        }

        RestartGameClientRpc(flatRealBoard, flatSpecial);
    }

    [ClientRpc]
    private void RestartGameClientRpc(int[] flatRealBoard, int[] flatSpecial)
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);

        isGameOver = false;
        hasLockedLocalTile = false;
        hasConfirmedThisTurn = false;
        hasResolvedThisTurn = false;
        emptyTilesCount = 16;
        p1Score = 0;
        p2Score = 0;
        p1CatchAmmo = 3;
        p2CatchAmmo = 3;
        currentTurnPlayer = 1;
        isWaitingForAction = false;
        temporarilySelectedFakeValue = 1;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (actionControlPanel != null) actionControlPanel.SetActive(false);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.text = "";

        ClearBoard(fakeBoard);
        ClearBoard(ownerBoard);
        ClearBoard(tileStatus);
        ClearBoard(specialType);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                realBoard[i, j] = flatRealBoard[i * 4 + j];
                specialType[i, j] = flatSpecial[i * 4 + j];
            }
        }

        CalculateAndDisplaySums();
        UpdateScoreUI();
        UpdateTurnUI();
        UpdateAmmoUI();
        UpdateUI();
    }

    private void ClearBoard(int[,] board)
    {
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                board[i, j] = 0;
    }

    private void CalculateAndDisplaySums()
    {
        for (int i = 0; i < 4; i++)
        {
            int rowSum = 0;
            for (int j = 0; j < 4; j++) rowSum += realBoard[i, j];
            if (rowSumTexts != null && rowSumTexts[i] != null)
                rowSumTexts[i].text = rowSum.ToString();
        }

        for (int j = 0; j < 4; j++)
        {
            int colSum = 0;
            for (int i = 0; i < 4; i++) colSum += realBoard[i, j];
            if (colSumTexts != null && colSumTexts[j] != null)
                colSumTexts[j].text = colSum.ToString();
        }
    }

    public void OnTileSelectedByPlayer(int x, int y)
    {
        if (isGameOver) return; 
        int myPlayerId = IsServer ? 1 : 2;
        if (currentTurnPlayer != myPlayerId) return; 
        if (challengeGroupObj != null && challengeGroupObj.activeSelf) return;
        if (ownerBoard[x, y] != 0) return; 
        if (hasLockedLocalTile && !lieGroupObj.activeSelf) return;

        hasLockedLocalTile = true;
        SelectTileServerRpc(x, y); 
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectTileServerRpc(int x, int y) { SelectTileClientRpc(x, y); }

    [ClientRpc]
    private void SelectTileClientRpc(int x, int y)
    {
        if (isWaitingForAction)
        {
            int oldTileIndex = selectedX * 5 + selectedY;
            allTiles[oldTileIndex].SetTileData(realBoard[selectedX, selectedY], fakeBoard[selectedX, selectedY], ownerBoard[selectedX, selectedY], tileStatus[selectedX, selectedY], specialType[selectedX, selectedY]);
        }

        selectedX = x; selectedY = y;
        isWaitingForAction = true;
        temporarilySelectedFakeValue = 1; 
        hasConfirmedThisTurn = false;

        int myPlayerId = IsServer ? 1 : 2;
        
        if (currentTurnPlayer == myPlayerId)
        {
            if (actionControlPanel != null) actionControlPanel.SetActive(true);
            if (lieGroupObj != null) lieGroupObj.SetActive(true);
            if (confirmButton != null) confirmButton.SetActive(true);
            if (challengeGroupObj != null) challengeGroupObj.SetActive(false);
        }
        else
        {
            if (actionControlPanel != null) actionControlPanel.SetActive(false);
        }
        
        if (ammoStatusText != null) ammoStatusText.text = "";

        int newTileIndex = selectedX * 5 + selectedY;
        allTiles[newTileIndex].SetTileData(realBoard[selectedX, selectedY], temporarilySelectedFakeValue, currentTurnPlayer, 0, specialType[selectedX, selectedY]);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StartTurnTimer(true)); 
    }

    public void OnSubmitFakeValue(int fakeVal)
    {
        if (isGameOver) return;
        int myPlayerId = IsServer ? 1 : 2;
        if (currentTurnPlayer != myPlayerId) return;
        if (hasConfirmedThisTurn) return; 

        SubmitFakeValueServerRpc(fakeVal);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitFakeValueServerRpc(int fakeVal) { SubmitFakeValueClientRpc(fakeVal); }

    [ClientRpc]
    private void SubmitFakeValueClientRpc(int fakeVal)
    {
        temporarilySelectedFakeValue = fakeVal;
        int tileIndex = selectedX * 5 + selectedY;
        allTiles[tileIndex].SetTileData(realBoard[selectedX, selectedY], fakeVal, currentTurnPlayer, 0, specialType[selectedX, selectedY]);
    }

    public void OnConfirmFakeValue()
    {
        if (isGameOver) return;
        int myPlayerId = IsServer ? 1 : 2;
        if (currentTurnPlayer != myPlayerId) return;
        if (hasConfirmedThisTurn) return;
        hasConfirmedThisTurn = true;

        if (confirmButton != null) confirmButton.SetActive(false);
        if (lieGroupObj != null) lieGroupObj.SetActive(false);

        ConfirmFakeValueServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ConfirmFakeValueServerRpc() { ConfirmFakeValueClientRpc(); }

    [ClientRpc]
    private void ConfirmFakeValueClientRpc()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        fakeBoard[selectedX, selectedY] = temporarilySelectedFakeValue;
        ownerBoard[selectedX, selectedY] = currentTurnPlayer;
        UpdateUI();
        UpdateAmmoUI();

        hasResolvedThisTurn = false;

        int myPlayerId = IsServer ? 1 : 2;
        int opponent = currentTurnPlayer == 1 ? 2 : 1;

        if (myPlayerId == opponent)
        {
            if (actionControlPanel != null) actionControlPanel.SetActive(true);
            if (lieGroupObj != null) lieGroupObj.SetActive(false);
            if (confirmButton != null) confirmButton.SetActive(false);
            if (challengeGroupObj != null) challengeGroupObj.SetActive(true);
        }
        else
        {
            if (actionControlPanel != null) actionControlPanel.SetActive(false);
        }

        timerCoroutine = StartCoroutine(StartTurnTimer(false));
    }

    public void OnResolveChallenge(bool didChallenge)
    {
        if (isGameOver) return;
        int myPlayerId = IsServer ? 1 : 2;
        int opponent = currentTurnPlayer == 1 ? 2 : 1;
        if (myPlayerId != opponent) return; 
        if (hasResolvedThisTurn) return;
        hasResolvedThisTurn = true;

        if (challengeGroupObj != null) challengeGroupObj.SetActive(false);

        ResolveChallengeServerRpc(didChallenge);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResolveChallengeServerRpc(bool didChallenge) { ResolveChallengeClientRpc(didChallenge); }

    [ClientRpc]
    private void ResolveChallengeClientRpc(bool didChallenge)
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

        int multiplier = 1;
        if (specialType[selectedX, selectedY] == 1) multiplier = 2;
        else if (specialType[selectedX, selectedY] == 2) multiplier = 3;

        if (didChallenge)
        {
            if (opponent == 1) p1CatchAmmo++;
            else p2CatchAmmo++;

            if (realVal != fakeVal)
            {
                tileStatus[selectedX, selectedY] = 2;
                int points = realVal * multiplier;

                if (specialType[selectedX, selectedY] == 2)
                    points = realVal * 2;

                if (currentTurnPlayer == 1)
                {
                    p1Score -= points;
                    p2Score += points;
                }
                else
                {
                    p2Score -= points;
                    p1Score += points;
                }
                ownerBoard[selectedX, selectedY] = opponent;
            }
            else
            {
                tileStatus[selectedX, selectedY] = 3;
                int penalty = (fakeVal + 10) * multiplier;
                if (currentTurnPlayer == 1)
                {
                    p1Score += (realVal * 2 * multiplier);
                    p2Score -= penalty;
                }
                else
                {
                    p2Score += (realVal * 2 * multiplier);
                    p1Score -= penalty;
                }
            }
        }
        else
        {
            tileStatus[selectedX, selectedY] = 1;
            if (currentTurnPlayer == 1)
                p1Score += (fakeVal * multiplier);
            else
                p2Score += (fakeVal * multiplier);
        }

        emptyTilesCount--;
        CalculateAndDisplaySums();
        UpdateScoreUI();
        UpdateUI();
        UpdateAmmoUI();

        int tileIndex = selectedX * 5 + selectedY;
        if (!didChallenge)
            allTiles[tileIndex].PlayPassAnimation();
        else if (realVal != fakeVal)
            allTiles[tileIndex].PlayCatchSuccessAnimation();
        else
            allTiles[tileIndex].PlayCatchFailAnimation();

        if (CheckGameOver())
            TriggerGameOver();
        else
            StartCoroutine(EndTurnSequence(opponent));
    }

    private IEnumerator StartTurnTimer(bool isSelectionPhase)
    {
        int timeLeft = emptyTilesCount >= 8 ? 12 : (emptyTilesCount >= 5 ? 8 : 5);

        while (timeLeft > 0)
        {
            if (timerText != null)
            {
                timerText.text = timeLeft.ToString();
                timerText.color = (emptyTilesCount <= 4) ? Color.red : Color.white;
            }
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (timerText != null) timerText.text = "0";

        if (IsServer)
        {
            if (isSelectionPhase && !hasConfirmedThisTurn)
                ConfirmFakeValueServerRpc();
            else if (!isSelectionPhase && !hasResolvedThisTurn)
                ResolveChallengeServerRpc(false);
        }
    }

    private IEnumerator EndTurnSequence(int nextPlayer)
    {
        yield return new WaitForSeconds(1.5f);
        if (isGameOver) yield break; 
        
        if (timerText != null) timerText.text = "";
        
        currentTurnPlayer = nextPlayer;
        isWaitingForAction = false; 
        hasLockedLocalTile = false; 

        UpdateTurnUI();
        UpdateUI();
    }

    private bool CheckGameOver()
    {
        return emptyTilesCount <= 0;
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        if (timerText != null) timerText.text = "";
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        int myId = IsServer ? 1 : 2;
        string winnerText = p1Score > p2Score ? "PLAYER 1 WIN!" : (p2Score > p1Score ? "PLAYER 2 WIN!" : "TIE!");
        
        if ((p1Score > p2Score && myId == 1) || (p2Score > p1Score && myId == 2)) 
            winnerText += "\n<color=yellow>CONGRARULATION!</color>";
        else if (p1Score != p2Score) 
            winnerText += "\n<color=red>LOSE!</color>";

        turnNotificationText.text = winnerText;
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null) p1ScoreText.text = "Player 1: " + p1Score.ToString();
        if (p2ScoreText != null) p2ScoreText.text = "Player 2: " + p2Score.ToString();
    }

    private void UpdateTurnUI()
    {
        if (turnNotificationText != null && !isGameOver)
        {
            int myId = IsServer ? 1 : 2;
            string prefix = (myId == currentTurnPlayer) ? "<color=yellow>Your turn</color> - " : "<color=red>Enemy turn</color> - ";
            turnNotificationText.text = $"{prefix}Player {currentTurnPlayer}";
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoStatusText == null) return;

        string p1Text = $"Player 1 : {p1CatchAmmo}";
        string p2Text = $"Player 2 : {p2CatchAmmo}";

        Color p1Color = p1CatchAmmo > 0 ? Color.green : Color.red;
        Color p2Color = p2CatchAmmo > 0 ? Color.green : Color.red;

        ammoStatusText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(p1Color)}>{p1Text}</color>  |  <color=#{ColorUtility.ToHtmlStringRGB(p2Color)}>{p2Text}</color>";
    }

    private void UpdateUI()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                int tileIndex = i * 5 + j;
                allTiles[tileIndex].SetTileData(realBoard[i, j], fakeBoard[i, j], ownerBoard[i, j], tileStatus[i, j], specialType[i, j]);
            }
        }
    }
}