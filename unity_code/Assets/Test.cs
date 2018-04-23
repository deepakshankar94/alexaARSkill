using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

    public Board board;
    public GameObject ImageTarget;
    public string code;
    public FirebaseDB firebase;

    private void Awake()
    {
        //ImageTarget.SetActive(false);
    }

    [ContextMenu("Test")]
    private void TestFunc()
    {
        firebase.gameReference.Child(code + "/moves").ChildAdded += MovesAdded;
    }

    void MovesAdded(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        Debug.Log("TEST");
        Debug.Log(args.Snapshot.GetRawJsonValue());
    }

    public void FetchFromDB()
    {
        firebase.FetchGameData(code);

        firebase.FetchedGameData = (gameData) =>
        {
            if (gameData == null)
            {
                Debug.LogError("Error fetching game data");
                return;
            }
            ImageTarget.SetActive(true);
            board.SetUpBoard(gameData);
        };
    }
}
