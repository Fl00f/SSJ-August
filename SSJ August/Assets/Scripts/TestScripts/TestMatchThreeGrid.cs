using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Random = System.Random;
using System.Linq;

public class TestMatchThreeGrid : MonoBehaviour {

    public Canvas GameCanvas;
    public Canvas StartMenuCanvas;

    public Canvas Losecanvas;
    public Canvas Wincanvas;

    public Shooter playerShooter;
    public Shooter aiShooter;

    private bool onLoseScene = false;
    private bool onWinScene = false;

    private bool gameOn;
    public bool GameOn {
        get { return gameOn; }
        set {
            if (value && value != gameOn) {
                StartGame ();
            } else if (!value && value != gameOn) {
                endGame ();
            }

            gameOn = value;
        }
    }
    private int playerShotCounter;
    public Text playerShotCount;

    public int PlayerShotCounter {
        get { return playerShotCounter; }
        set {
            playerShotCounter = value;
            playerShotCount.text = playerShotCounter.ToString ();
        }
    }

    private int aiShotCounter;
    public Text AIShotCount;

    public int AIShotCounter {
        get { return aiShotCounter; }
        set {
            aiShotCounter = value;
            AIShotCount.text = aiShotCounter.ToString ();
        }
    }

    private int playerHealth;
    public Text playerHealthText;

    public int PlayerHealth {
        get { return playerHealth; }
        set {
            if (value > 0) {
                playerHealth = value;
            } else {
                if (!onWinScene && !onLoseScene) {
                    loseGameState ();
                }
                playerHealth = 0;
            }

            playerHealthText.text = playerHealth.ToString ();
        }
    }

    private int aiHealth;
    public Text aiHealthText;

    public int AIHealth {
        get { return aiHealth; }
        set {
            if (value > 0) {
                aiHealth = value;
            } else {
                if (!onWinScene && !onLoseScene) {
                    winGameState ();
                }
                aiHealth = 0;
            }
            aiHealthText.text = aiHealth.ToString ();
        }
    }

    private int turnCounter;
    public Text turnCount;

    public int TurnCounter {
        get { return turnCounter; }
        set {
            turnCounter = value;

            if (turnCounter >= TurnsToActivateShooting) {
                startCombat ();
                turnCounter = 0;
            }
            turnCount.text = turnCounter.ToString ();

        }
    }

    public int DifficultyRampInSeconds = 60;

    public int TurnsToActivateShooting = 4;
    [Range (0, 1)] public float PlaySuccessRate;

    [Range (0, 1)] public float AIStartAccuracy;
    [Range (0, 1)] public float AIMaxAccuracy;
    private float AIShotAccuracy {
        get {
            float dif = Time.time - gameStartTime;
            float percentage = Mathf.Clamp (dif / (float) DifficultyRampInSeconds, AIStartAccuracy, AIMaxAccuracy);
            return percentage;
        }
    }

    public int maxSecondForAIShotCount = 10;
    public int minSecondForAIShotCount = 1;

    private int AiGetShotEveryNSecond {
        get {
            float dif = Time.time - gameStartTime;
            float percentage = Mathf.Clamp (dif / (float) DifficultyRampInSeconds, 0f, 1f);

            float timePerShotCounter = Mathf.Clamp ((1f - percentage) * maxSecondForAIShotCount, minSecondForAIShotCount, maxSecondForAIShotCount + 1);
            return (int) timePerShotCounter;
        }
    }

    private float gameStartTime;

    public static int RowCutOff;
    public Transform[] gridColumns;
    private int ColumnLength => gridColumns.Length;

    public Vector2[, ] gridImagePositions;
    public Vector2[, ] translationImageStartPositions;

    public float translationTime => doingSwap ? swapTime : normalTranslationTime;

    public float normalTranslationTime;
    public float swapTime;
    private bool doingSwap;

    private Action OnTranslationEnd;

    public int minNumberOfConnections = 3;

    public static Dictionary<TileType, Sprite> TileTypeDictionary;

    private bool doImageTranslations;
    private float translationStartTime;

    private bool isInCombat;

    public Sprite tileTypeOne;
    public Sprite tileTypeTwo;
    public Sprite tileTypeThree;
    public Sprite tileTypeFour;

    // Use this for initialization
    void Start () {
        RowCutOff = 10;
        TileTypeDictionary = new Dictionary<TileType, Sprite> () { { TileType.Red, tileTypeOne }, { TileType.Blue, tileTypeTwo }, { TileType.Green, tileTypeThree }, { TileType.Purple, tileTypeFour },
        };
        SetInitialGridPositions ();
        GameCanvas.enabled = false;
    }

    private float aiTurnTimeCounter = 0;
    // Update is called once per frame
    void Update () {

        if (doImageTranslations && translationImageStartPositions != null) {
            float percentage = (Time.time - translationStartTime) / translationTime;
            float percentageClamp = Mathf.Clamp (percentage, 0, 1);
            translateImagesToGridPositions (percentageClamp);

            if (percentageClamp >= 1) {
                doImageTranslations = false;
                translationImageStartPositions = null;
                doingSwap = false;
                OnTranslationEnd?.Invoke ();
            }
        }

        aiTurnTimeCounter += Time.deltaTime;

        if (gameOn && aiTurnTimeCounter > AiGetShotEveryNSecond) {
            aiTurnTimeCounter = 0;
            AIShotCounter++;
        }
    }

    private void StartGame () {
        GameCanvas.enabled = true;
        StartMenuCanvas.enabled = false;
        resetGame ();
        gameStartTime = Time.time;
    }

    private void resetGame () {
        AIShotCounter = 0;
        PlayerShotCounter = 0;

        TurnCounter = 0;

        PlayerHealth = 10;
        AIHealth = 10;

        aiTurnTimeCounter = 0;

        gameStartTime = 0;

        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest> ();
        allObjects.ToList ().ForEach (obj => obj.ResetConnections ());

        Wincanvas.enabled = false;
        Losecanvas.enabled = false;
    }

    private void loseGameState () {
        onLoseScene = true;
        Losecanvas.enabled = true;
    }

    private void winGameState () {
        onWinScene = true;
        Wincanvas.enabled = true;
    }

    private void endGame () {
        resetGame ();
        GameCanvas.enabled = false;
        StartMenuCanvas.enabled = true;
        Wincanvas.enabled = false;
        Losecanvas.enabled = false;
    }

    private void translateImagesToGridPositions (float translationPercentage) {
        for (int i = 0; i < ColumnLength; i++) {
            for (int j = 0; j < gridColumns[0].childCount; j++) {
                Transform imageTransform = gridColumns[i].GetChild (j);

                Vector3 lerpPos = Vector3.Lerp (translationImageStartPositions[i, j], gridImagePositions[i, j],
                    translationPercentage);

                imageTransform.position = lerpPos;
            }
        }
    }

    private Vector2[, ] getCurrentImagePositions () {
        Vector2[, ] currentImageStartPositions = new Vector2[ColumnLength, gridColumns[0].childCount];

        for (int i = 0; i < ColumnLength; i++) {
            for (int j = 0; j < gridColumns[0].childCount; j++) {
                Vector3 pos = gridColumns[i].GetChild (j).position;

                currentImageStartPositions[i, j] = new Vector2Int ((int) pos.x, (int) pos.y);
            }
        }

        return currentImageStartPositions;
    }

    private void SetInitialGridPositions () {
        //using the pre established positions from prefab, record initial positions into grid
        gridImagePositions = new Vector2[ColumnLength, gridColumns[0].childCount];
        int tileTypeMax = Enum.GetValues (typeof (TileType)).Length;
        Random ran = new Random ();;
        for (int i = 0; i < ColumnLength; i++) {
            for (int j = 0; j < gridColumns[0].childCount; j++) {

                ObjectDragTest dragableObj = gridColumns[i].GetChild (j).GetComponent<ObjectDragTest> ();
                dragableObj.OnSwap += handleTileSwap;
                dragableObj.TileType = (TileType) ran.Next (0, tileTypeMax);

                Vector3 pos = gridColumns[i].GetChild (j).position;

                gridImagePositions[i, j] = new Vector2 (pos.x, pos.y);
            }
        }
    }

    private void handleTileSwap (ObjectDragTest a, ObjectDragTest b) {
        doingSwap = true;
        translationImageStartPositions = getCurrentImagePositions ();
        translationStartTime = Time.time;
        doImageTranslations = true;

        //swap positions (only grid positions)
        a.swapPositionsWith (b);
        //go through connections
        setConnectionRoot (a, a);
        setConnectionRoot (b, b);

        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest> ();
        //if number of connections is zero swap back
        if (allObjects.Count (obj => obj.Connections.Count + 1 >= minNumberOfConnections) == 0 ||
            a.TileType == b.TileType) {
            //swap back
            a.swapPositionsWith (b);
            allObjects.ToList ().ForEach (obj => obj.ResetConnections ());
        }

        OnTranslationEnd += handleSwapTranslationEnd;
    }

    private void handleSwapTranslationEnd () {
        doingSwap = false;

        ObjectDragTest[] allObjects = FindObjectsOfType<ObjectDragTest> ();
        int connections = 0;
        bool addToTurnCount = false;
        //we add one because root is not part of the connection nodes
        foreach (var connectionRoot in allObjects.Where (a => a.Connections.Count + 1 >= minNumberOfConnections)) {
            addToTurnCount = true;
            connections++;
            //push connections up super high out of sight and make first child of parent
            for (int i = 0; i < connectionRoot.Connections.Count; i++) {
                connectionRoot.Connections[i].transform.SetSiblingIndex (0);
                Vector3 posInner = connectionRoot.Connections[i].transform.position;

                posInner.y = 1000;

                connectionRoot.Connections[i].transform.position = posInner;
                connectionRoot.Connections[i].GetComponent<Image> ().enabled = false;

                int numberOfTiles = Enum.GetValues (typeof (TileType)).Length;
                connectionRoot.Connections[i].TileType = (TileType) UnityEngine.Random.Range (0, numberOfTiles);

                connections++;
            }

            connectionRoot.transform.SetSiblingIndex (0);
            Vector3 pos = connectionRoot.transform.position;

            pos.y = 1000;

            connectionRoot.transform.position = pos;
        }

        PlayerShotCounter += Mathf.FloorToInt ((float) (connections / 3));

        if (addToTurnCount) {
            TurnCounter++;
        }

        //reset connections
        allObjects.ToList ().ForEach (obj => obj.ResetConnections ());

        //since connections have been moved we need to translate again to push tiles down
        translationImageStartPositions = getCurrentImagePositions ();
        translationStartTime = Time.time;
        doImageTranslations = true;

        OnTranslationEnd += () => {
            ObjectDragTest[] allDaObjects = FindObjectsOfType<ObjectDragTest> ();
            foreach (var item in allDaObjects) {
                item.GetComponent<Image> ().enabled = true;
            }

            OnTranslationEnd = null;
        };

    }

    private void setConnectionRoot (ObjectDragTest a, ObjectDragTest root) {
        for (int i = 0; i < 4; i++) {
            ObjectDragTest connection = a.getConnectingObj (i);

            if (connection != null && !connection.IsConnected && connection.TileType == a.TileType) {
                root.AddConnection (connection);
                setConnectionRoot (connection, root);
            }
        }
    }

    public void HandlePlayerHit () {
        PlayerHealth--;
    }

    public void HandleAIHit () {
        AIHealth--;
    }

    private void startCombat () {
        playerShooter.StartShooting (PlayerShotCounter, PlaySuccessRate, .5f);
        aiShooter.StartShooting (AIShotCounter, AIShotAccuracy, .5f);

        PlayerShotCounter = 0;
        AIShotCounter = 0;
    }

    private void handleCombat () {

    }
}