using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using SimpleJSON;

public class FirebaseDB : MonoBehaviour {

    public GameManager gameManager;
    public RandomCodeGenerator codeGenerator;
    public Action<string> CodeGenerated = delegate { };
    public Action<string> AlexaPaired = delegate { };
    public Action GameStarted = delegate { };

    public Action<GameCollection> FetchedGameData = delegate { };

    public GameObject boardPrefab;

    public DatabaseReference alexaReference;
    public DatabaseReference gameReference;
    public DatabaseReference rootReference;

    private int noOfAttempts = 0, maxNoOfAttempts = 5;

    private void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://wizardchess-a4c9e.firebaseio.com/");

        //Test
        //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://testwizardchess.firebaseio.com/");

        rootReference = FirebaseDatabase.DefaultInstance.RootReference;
        alexaReference = FirebaseDatabase.DefaultInstance.GetReference("alexaColl");
        gameReference = FirebaseDatabase.DefaultInstance.GetReference("gameColl");

        if (PlayerPrefs.HasKey("AlexaCode"))
        {
            alexaReference.Child(PlayerPrefs.GetString("AlexaCode")).ChildChanged += HandleAlexaCollectionChange;
        }
    }

    void SetUpBoard(JSONArray squares)
    {
        GameObject boardObject = Instantiate(boardPrefab);
        Board board = boardObject.GetComponent<Board>();
        //board.SetUpPieces(squares);
    }
    
    public void CreateAlexaColl(string id)
    {
        string code = codeGenerator.Generate();

        alexaReference.Child(code).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database Error");
            }
            else if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    Debug.Log("Exists");
                    if (++noOfAttempts < maxNoOfAttempts)
                    {
                        Debug.Log("Attempting.. " + noOfAttempts);
                        CreateAlexaColl(id);
                    }
                    else
                    {
                        CodeGenerated(null);
                        Debug.Log("Cannot find a unique code");
                    }
                }
                else
                {
                    noOfAttempts = 0;
                    Debug.Log("New code");
                    AlexaCollection collection = new AlexaCollection(id);

                    string json = JsonUtility.ToJson(collection);
                    Debug.Log(json);

                    alexaReference.Child(code).SetRawJsonValueAsync(json);
                    CodeGenerated(code);

                    alexaReference.Child(code).ChildChanged += HandleAlexaCollectionChange;
                }
            }
        });
    }

    void HandleAlexaCollectionChange(object sender, ChildChangedEventArgs args)
    {
        Debug.Log("Alexa Collection changed! " + args.Snapshot.Key);
        Debug.Log(args.Snapshot.GetRawJsonValue());
        if (args.Snapshot.Key.ToString() == "alexaId")
        {
            Debug.Log("Alexa paired " + args.Snapshot.Value);
            AlexaPaired(args.Snapshot.Value.ToString());
            return;
        }
        else if (args.Snapshot.Key.ToString() == "currentGameCode")
        {
            Debug.Log("Start Game " + args.Snapshot.Value);
            GameStarted();

            PlayerPrefs.SetString("CurrentGameCode", args.Snapshot.Value.ToString());
            Debug.Log(PlayerPrefs.GetString("CurrentGameCode"));
            return;
        }
        else if (args.Snapshot.Key.ToString() == "updatedAt")
        {
            GameStarted();
        }
    }

    public void CreateGameColl(string id, bool isSameAlexa)
    {
        string code = codeGenerator.Generate();

        gameReference.Child(code).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database Error");
            }
            else if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    Debug.Log("Exists");
                    if (++noOfAttempts < maxNoOfAttempts)
                    {
                        Debug.Log("Attempting.. " + noOfAttempts);
                        CreateGameColl(id, isSameAlexa);
                    }
                    else
                    {
                        CodeGenerated(null);
                        Debug.Log("Cannot find a unique code");
                    }
                }
                else
                {
                    noOfAttempts = 0;
                    Debug.Log("New code");
                    GameCollection collection = new GameCollection(isSameAlexa);

                    string json = JsonUtility.ToJson(collection);
                    Debug.Log(json);

                    //alexaReference.Child(PlayerPrefs.GetString("AlexaCode") + "/currentGameCode").SetValueAsync(code);
                    gameReference.Child(code).SetRawJsonValueAsync(json);
                    CodeGenerated(code);
                }
            }
        });
    }

    public void FetchGameData(string code)
    {
        gameReference.Child(code).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Database Error!");
                FetchedGameData(null);
            }
            if (task.IsCompleted)
            {
                var node = JSON.Parse(task.Result.GetRawJsonValue());
                Debug.Log(task.Result.GetRawJsonValue());
                Debug.Log(node["moves"]);
                GameCollection gameData = new GameCollection(node["black"], node["board"], node["currentTurn"], node["moves"], node["isCompleted"].AsBool, node["white"], node["winner"]);
                FetchedGameData(gameData);
            }
        });
    }

    public void UpdateGameData(string code, string pieceName, PieceMove move, int fromIndex, int toIndex)
    {
        Debug.Log(JsonUtility.ToJson(move));

        //string key = gameReference.Child("/" + code + "/moves").Push().Key;
        int key = gameManager.currentGameData.moves.Count;
        string currentTurn = (gameManager.currentGameData.currentTurn == "white") ? "black" : "white";

        Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
        childUpdates["/moves/" + key] = move.ToDictionary();
        childUpdates["/board/" + fromIndex] = "";
        childUpdates["/board/" + toIndex] = pieceName;
        childUpdates["/currentTurn"] = currentTurn;

        gameReference.Child(code).UpdateChildrenAsync(childUpdates);
    }

    public void UpdateBoardData(string code, string pieceName, int fromIndex, int toIndex)
    {
        Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();

        childUpdates["/board/" + fromIndex] = "";
        childUpdates["/board/" + toIndex] = pieceName;

        gameReference.Child(code).UpdateChildrenAsync(childUpdates);
    }

    // TEST
    public string from;
    public string to;
    public string piece_name;

    [ContextMenu("Test Update Move")]
    void TestUpdateGameData()
    {
        //UpdateGameData(PlayerPrefs.GetString("CurrentGameCode"), piece_name, new PieceMove(from, to), Coordinate.GetCoordinatePosition(from).GetSquareListIndex(), Coordinate.GetCoordinatePosition(to).GetSquareListIndex());
        PieceMove move = new PieceMove(from, to);
        int key = gameManager.currentGameData.moves.Count;
        string currentTurn = (gameManager.currentGameData.currentTurn == "white") ? "black" : "white";

        Dictionary<string, System.Object> childUpdates = new Dictionary<string, System.Object>();
        childUpdates["/moves/" + key] = move.ToDictionary();
        childUpdates["/currentTurn"] = currentTurn;

        gameReference.Child(PlayerPrefs.GetString("CurrentGameCode")).UpdateChildrenAsync(childUpdates);
    }
}