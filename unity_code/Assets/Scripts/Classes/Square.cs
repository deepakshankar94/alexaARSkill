using UnityEngine;
using System;
using System.Collections;

/*
==============================
[Square] - Script placed on every square in the board.
==============================
*/
public class Square : MonoBehaviour {
    private Material cur_mat; // Current material

    [SerializeField]
    public Coordinate coor; // Square position in the board
    public Piece holding_piece = null; // Current piece in this square
    public Material start_mat; // Default material
    public ParticleSystem particleSystem;
    public ParticleSystem dieEffect;

    [SerializeField]
    public int team;

    [SerializeField]
    public Board board;

    void Start() {
        start_mat = GetComponent<Renderer>().material;
    }

    public void holdPiece(Piece piece) {
        holding_piece = piece;
    }

    /*
    ---------------
    Materials related functions
    ---------------
    */ 
    public void hoverSquare(Material mat) {
        cur_mat = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = mat;
    }

    public void unHoverSquare() {
        GetComponent<Renderer>().material = cur_mat;
    }

    // Reset material to default
    public void resetMaterial() {
        cur_mat = start_mat;
        GetComponent<Renderer>().material = start_mat;
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse down on " + gameObject.name);
        //if (holding_piece != null)
        //{
        //    board.cur_piece = holding_piece;
        //    if (board.use_hover)
        //    {
        //        board.hoverValidSquares(holding_piece);
        //    }
        //}
        if (board.cur_piece != null)
        {
            Debug.Log("Moving current piece " + board.cur_piece);
            if (board.cur_piece.checkValidMove(this))
            {
                board.cur_move = this;
            }
            board.cur_piece.movePiece(this);
            board.cur_piece = null;
            board.resetHoveredSquares();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Piece"))
        {
            Piece piece = other.GetComponent<Piece>();
            if (holding_piece != null && piece.cur_square != this)
            {
                Debug.Log("Hiding " + holding_piece.name);
                board.hidden_Pieces.Add(holding_piece);
                //holding_piece.gameObject.SetActive(false);
                StartCoroutine(TogglePieceDisplay(false));
            }
        }
    }

    public IEnumerator TogglePieceDisplay(bool toggle)
    {
        particleSystem.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        holding_piece.gameObject.SetActive(toggle);
        yield return new WaitForSeconds(2f);
        particleSystem.gameObject.SetActive(false);
    }

    public IEnumerator Die()
    {
        Debug.Log("DIEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        dieEffect.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.4f);

        dieEffect.gameObject.SetActive(false);

        Debug.Log(dieEffect.gameObject.active);
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Piece"))
    //    {
    //        Debug.Log(other.gameObject.name + " Exited " + gameObject.name);
    //        Piece piece = other.GetComponent<Piece>();
    //        if (holding_piece != null && piece.cur_square != this)
    //        {
    //            //Debug.Log("Showing " + holding_piece.name);
    //            //holding_piece.gameObject.SetActive(true);
    //            //StartCoroutine(TogglePieceDisplay(true));
    //        }
    //    }
    //}
}
