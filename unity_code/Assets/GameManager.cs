using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleJSON;

public class AlexaCollection
{
    public string deviceId;
    public string alexaId;
    public string currentGameCode;

    public AlexaCollection(string id)
    {
        deviceId = id;
        currentGameCode = null;
        alexaId = null;
    }
}

public class GameCollection
{
    public string currentTurn;
    public string black;
    public string white;
    public string[] board;
    public bool isCompleted;
    public bool isSameAlexa;
    public string winner;
    public List<PieceMove> moves = new List<PieceMove>();

    public GameCollection(bool isSameAlexa)
    {
        white = "";
        currentTurn = "white";
        isCompleted = false;
        this.isSameAlexa = isSameAlexa;
        winner = "";
        board = new string[] { "w_rook", "w_horse", "w_bishop", "w_queen", "w_king", "w_bishop", "w_horse", "w_rook", "w_pawn", "w_pawn", "w_pawn", "w_pawn", "w_pawn", "w_pawn", "w_pawn", "w_pawn", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "b_pawn", "b_pawn", "b_pawn", "b_pawn", "b_pawn", "b_pawn", "b_pawn", "b_pawn", "b_rook", "b_horse", "b_bishop", "b_queen", "b_king", "b_bishop", "b_horse", "b_rook"};
    }

    public GameCollection(string black, JSONNode board, string currentTurn, JSONNode moves, bool isCompleted, string white, string winner)
    {
        this.black = black;
        this.currentTurn = currentTurn;
        this.isCompleted = isCompleted;
        this.white = white;
        this.winner = winner;

        UpdateBoard(board);
        //UpdateMoves(moves);
    }

    public void UpdateMoves(JSONNode moveList)
    {
        foreach (var move in moveList.Children)
        {
            moves.Add(new PieceMove(move["from"], move["to"], move["updatedWithAlexa"], move["updatedWithDevice"]));
        }
    }

    public void UpdateBoard(JSONNode array)
    {
        List<string> board = new List<string>();
        foreach (var item in array.Children)
        {
            board.Add(item);
        }
        this.board = board.ToArray();
    }
}

public class PieceMove
{
    public string from;
    public string to;
    public bool updatedWithAlexa;
    public bool updatedWithDevice;

    public PieceMove(string f, string t)
    {
        from = f;
        to = t;
    }
    public PieceMove(string f, string t, bool device)
    {
        from = f;
        to = t;
        updatedWithDevice = device;
    }
    public PieceMove(string f, string t, bool alexa, bool device)
    {
        from = f;
        to = t;
        updatedWithDevice = device;
        updatedWithAlexa = alexa;
    }

    public Dictionary<string, System.Object> ToDictionary()
    {
        Dictionary<string, System.Object> result = new Dictionary<string, System.Object>();

        result["from"] = from;
        result["to"] = to;
        result["updatedWithAlexa"] = updatedWithAlexa;
        result["updatedWithDevice"] = updatedWithDevice;

        return result;
    }
}

public class GameManager : MonoBehaviour {

    public RandomCodeGenerator codeGenerator;
    public FirebaseDB firebase;

    public GameObject backButton;

    public GameObject mainMenuPanel;
    public GameObject connectAlexaPanel;
    public GameObject displayCodePanel;
    public NewGameMenu newGameMenu;
    public GameObject continueGamePanel;
    public JoinGameMenu joinGameMenu;

    public GameUI gameUI;

    public TextMeshProUGUI codeText;
    public TextMeshProUGUI statusText;

    public GameObject ARCamera;
    public GameObject NonARCamera;
    public GameObject ImageTarget;
    public bool isAR = false;

    public Board board;
    public GameCollection currentGameData;

    private string DEVICE_ID;

    private void Awake()
    {
        // Unique Device ID
        if (PlayerPrefs.HasKey("DEVICE_ID"))
        {
            Debug.Log("Device already registered");
            DEVICE_ID = PlayerPrefs.GetString("DEVICE_ID");
        }
        else
        {
            Debug.Log("Opening app for the first time! Registering the device.");
            DEVICE_ID = SystemInfo.deviceUniqueIdentifier;
            PlayerPrefs.SetString("DEVICE_ID", DEVICE_ID);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainMenuPanel.activeSelf)
            {
                Debug.Log("Quitting");
                Application.Quit();
            }
            else if (gameUI.gameObject.activeSelf)
            {
                Debug.Log("Menu button click");
                gameUI.menuButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            }
            else
            {
                Debug.Log("Back button click");
                backButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();   
            }
        }
    }

    [ContextMenu("Delete All PlayerPrefs")]
    void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void GenerateCodeForAlexa()
    {
        mainMenuPanel.SetActive(false);

        if (PlayerPrefs.HasKey("AlexaCode"))
        {
            connectAlexaPanel.SetActive(true);
            return;
        }

        firebase.CreateAlexaColl(DEVICE_ID);

        displayCodePanel.SetActive(true);

        statusText.text = "Getting code for you..";

        firebase.CodeGenerated = (code) => {
            if (string.IsNullOrEmpty(code))
            {
                codeText.text = "Cannot find a code!";
                return;
            }
            codeText.text = code.Replace('_', ' ');
            PlayerPrefs.SetString("AlexaCode", code);

            statusText.text = "Waiting for Alexa to pair..";

            firebase.AlexaPaired = (id) =>
            {
                statusText.text = "Alexa Paired!";
            };
        };
    }

    public void GenerateCodeForGame(bool isSameAlexa)
    {
        mainMenuPanel.SetActive(false);

        if (!PlayerPrefs.HasKey("AlexaCode"))
        {
            Debug.Log("Integrate Alexa before starting the game!");
            statusText.text = "Integrate Alexa before starting the game!";
            return;
        }

        displayCodePanel.SetActive(true);
        newGameMenu.gameObject.SetActive(true);

        statusText.text = "Getting code for you..";

        firebase.CreateGameColl(DEVICE_ID, isSameAlexa);

        firebase.CodeGenerated = (code) => {
            if (string.IsNullOrEmpty(code))
            {
                codeText.text = "Cannot find a code!";
                return;
            }

            codeText.text = code.Replace('_', ' ');

            firebase.GameStarted = () =>
            {
                Debug.Log("Game synced with Alexa");
                newGameMenu.startGameButton.SetActive(true);
            };
        };
    }

    public void OnClickContinueButton()
    {
        mainMenuPanel.SetActive(false);
        if (!PlayerPrefs.HasKey("AlexaCode"))
        {
            Debug.Log("Integrate Alexa before starting the game!");
            statusText.text = "Integrate Alexa before starting the game!";
            return;
        }

        continueGamePanel.SetActive(true);
    }

    public void OnClickJoinButton()
    {
        mainMenuPanel.SetActive(false);
        if (!PlayerPrefs.HasKey("AlexaCode"))
        {
            Debug.Log("Integrate Alexa before starting the game!");
            statusText.text = "Integrate Alexa before starting the game!";
            return;
        }

        joinGameMenu.gameObject.SetActive(true);

        firebase.GameStarted = () =>
        {
            Debug.Log("Game added by Alexa " + PlayerPrefs.GetString("CurrentGameCode"));
            joinGameMenu.joinButton.SetActive(true);
        };
    }

    public void StartGame(string gameCode)
    {
        gameCode = PlayerPrefs.GetString("CurrentGameCode");
        firebase.FetchGameData(gameCode);
        
        firebase.FetchedGameData = (gameData) =>
        {
            if (gameData == null)
            {
                Debug.LogError("Error fetching game data");
                return;
            }

            if (gameData.isCompleted)
            {
                string winner = gameData.winner;
                statusText.text = "Game completed. Winner is: " + winner;
                backButton.SetActive(true);
                return;
            }

            Debug.Log("Fetched game data in GameManager " + gameCode);

            // Keeping the copy of latest data
            currentGameData = gameData;

            gameUI.gameObject.SetActive(true);

            if (string.IsNullOrEmpty(gameData.black))
            {
                gameUI.waitingPanel.SetActive(true);
                firebase.gameReference.Child(gameCode + "/black").ValueChanged += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(args.Snapshot.Value.ToString()))
                    {
                        return;
                    }
                    Debug.Log(args.Snapshot.Key + " " + args.Snapshot.Value);
                    gameUI.waitingPanel.SetActive(false);
                    InitializeGame();
                };
            }
            else
            {
                InitializeGame();
            }
        };
    }

    void InitializeGame()
    {
        string gameCode = PlayerPrefs.GetString("CurrentGameCode");
        firebase.gameReference.Child(gameCode + "/moves").ChildAdded += MovesAdded;
        //firebase.gameReference.Child(gameCode).ChildChanged += GameDataUpdated;

        firebase.gameReference.Child(gameCode + "/currentTurn").ValueChanged += (sender, args) => {
            currentGameData.currentTurn = args.Snapshot.Value.ToString();
        };
        firebase.gameReference.Child(gameCode + "/isCompleted").ValueChanged += (sender, args) => {
            currentGameData.isCompleted = bool.Parse(args.Snapshot.Value.ToString());
        };
        firebase.gameReference.Child(gameCode + "/winner").ValueChanged += (sender, args) => {
            currentGameData.winner = args.Snapshot.Value.ToString();
            if (!string.IsNullOrEmpty(args.Snapshot.Value.ToString()))
            {
                board.winnerText.text = "Winner is: " + currentGameData.winner;
            }
        };

        ImageTarget.SetActive(true);

        board.PieceMoved = UpdateMove;
        board.SetUpBoard(currentGameData);

        if (!isAR)
        {
            Debug.Log("Camera toggle");
            ARCamera.GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
            ToggleCamera();
        }
    }

    public void JoinGame(string gameCode)
    {
        firebase.FetchGameData(gameCode);

        firebase.FetchedGameData = (gameData) =>
        {
            if (gameData == null)
            {
                Debug.LogError("Error fetching game data");
                return;
            }

            if (gameData.isCompleted)
            {
                string winner = (gameData.winner == PlayerPrefs.GetString("DEVICE_ID")) ? "You" : "Opponent";
                statusText.text = "Game completed. Winner is: " + winner;
                return;
            }

            PlayerPrefs.SetString("CurrentGameCode", gameCode);

            Debug.Log("Fetched game data in GameManager " + gameCode);

            // Keeping the copy of latest data
            currentGameData = gameData;

            firebase.gameReference.Child(gameCode + "/moves").ChildAdded += MovesAdded;
            //firebase.gameReference.Child(gameCode).ChildChanged += GameDataUpdated;

            firebase.gameReference.Child(gameCode + "/currentTurn").ValueChanged += (sender, args) => {
                currentGameData.currentTurn = args.Snapshot.Value.ToString();
            };
            firebase.gameReference.Child(gameCode + "/isCompleted").ValueChanged += (sender, args) => {
                currentGameData.isCompleted = bool.Parse(args.Snapshot.Value.ToString());
            };
            firebase.gameReference.Child(gameCode + "/winner").ValueChanged += (sender, args) => {
                currentGameData.winner = args.Snapshot.Value.ToString();
            };

            gameUI.gameObject.SetActive(true);
            ImageTarget.SetActive(true);

            board.PieceMoved = UpdateMove;
            board.SetUpBoard(gameData);

            if (!isAR)
            {
                Debug.Log("Camera toggle");
                ARCamera.GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
                ToggleCamera();
            }
        };
    }

    // Firebase callback on update of current game collection
    void GameDataUpdated(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("Game Data updated");
        Debug.Log(args.Snapshot.GetRawJsonValue());
        var node = JSON.Parse(args.Snapshot.GetRawJsonValue());
        currentGameData.currentTurn = node["currentTurn"];
    }

    void MovesAdded(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("Moves added");
        Debug.Log(args.Snapshot.GetRawJsonValue());
        PieceMove move = JsonUtility.FromJson<PieceMove>(args.Snapshot.GetRawJsonValue());
        
        if (!move.updatedWithDevice)
        {
            Debug.Log("Move the piece");

            int fromIndex = Coordinate.GetCoordinatePosition(move.from).GetSquareListIndex();
            int toIndex = Coordinate.GetCoordinatePosition(move.to).GetSquareListIndex();

            Square from = board.squares[fromIndex];
            Square to = board.squares[toIndex];

            string piece_name = currentGameData.board[fromIndex];
            currentGameData.board[fromIndex] = "";
            currentGameData.board[toIndex] = piece_name;

            Debug.Log(currentGameData.board[fromIndex] + " " + currentGameData.board[toIndex]);
            Debug.Log(piece_name + " " + fromIndex + " " + toIndex);

            from.holding_piece.movePiece(to, false);

            move.updatedWithDevice = true;

            // Updating the firebase
            firebase.gameReference.Child(PlayerPrefs.GetString("CurrentGameCode") + "/moves/" + (currentGameData.moves.Count) + "/updatedWithDevice").SetValueAsync(true);
            firebase.UpdateBoardData(PlayerPrefs.GetString("CurrentGameCode"), piece_name, fromIndex, toIndex);
        }

        currentGameData.moves.Add(move);
    }

    // Callback from the game after each moved is played
    void UpdateMove(string name, Square from, Square to)
    {
        Debug.LogFormat("Move added from {0} to {1}", from.coor.GetBoardPosition(), to.coor.GetBoardPosition());
        PieceMove move = new PieceMove(from.coor.GetBoardPosition(), to.coor.GetBoardPosition(), true);
        int fromIndex = from.coor.x * 8 + from.coor.y;
        int toIndex = to.coor.x * 8 + to.coor.y;
        firebase.UpdateGameData(PlayerPrefs.GetString("CurrentGameCode"), name, move, fromIndex, toIndex);
    }

    public void OnClickMenuButton()
    {
        if (isAR)
        {
            ToggleCamera();
        }

        firebase.gameReference.Child(PlayerPrefs.GetString("CurrentGameCode") + "/moves").ChildAdded -= MovesAdded;

        currentGameData = null;

        board.ResetBoard();
        board.PieceMoved = null;

        mainMenuPanel.SetActive(true);
        ImageTarget.SetActive(false);
        gameUI.gameObject.SetActive(false);
    }

    public void ToggleCamera()
    {
        isAR = !isAR;
        ARCamera.SetActive(isAR);
        NonARCamera.SetActive(!isAR);

        var rendererComponents = ImageTarget.GetComponentsInChildren<Renderer>(true);
        var colliderComponents = ImageTarget.GetComponentsInChildren<Collider>(true);
        var canvasComponents = ImageTarget.GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = !isAR;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = !isAR;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = !isAR;
    }
}