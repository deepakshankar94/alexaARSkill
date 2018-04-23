using UnityEngine;

/*
==============================
[DragAndDrop] - Script placed on every piece in the board.
==============================
*/
class DragAndDrop : MonoBehaviour {
    private bool dragging = false;
    private float distance;
    private Piece this_piece;

    private Vector3 previousPosition;

    [SerializeField]
    public Board board;

    void Start() {
        this_piece = GetComponent<Piece>(); // Get piece's component
        previousPosition = transform.position;
    }

    void Update() {
        if (dragging) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);

            // Update piece's dragging position, we try to place it as close as we can to the mouse
            transform.position = new Vector3(rayPoint.x - 0.05f, 0.15f, rayPoint.z);
            transform.rotation = new Quaternion(0, 0, 0, 0);

            // Hover the square this piece could go id we drop it
            if (board.use_hover) {
                Square closest_square = board.getClosestSquare(transform.position);
                board.hoverClosestSquare(closest_square);
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Mouse down at " + gameObject.name);
        // If it's my turn
        if (board.cur_turn == this_piece.team)
        {
            Debug.Log("Selected " + gameObject.name);
            //previousTransform = transform;
            //Debug.Log(previousTransform.position);
            //GetComponent<Rigidbody>().isKinematic = true;
            // Set distance between the mouse & this piece
            board.cur_piece = this_piece;
            //distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            if (board.use_hover)
            {
                board.resetHoveredSquares();
                board.hoverValidSquares(this_piece);
            }
            //dragging = true; // Start dragging
        }
        else if (board.cur_piece != null)
        {
            Debug.Log("Moving current piece " + board.cur_piece);
            board.cur_piece.movePiece(this_piece.cur_square);
            board.cur_piece = null;
            board.resetHoveredSquares();
        }
    }
    

    //void OnMouseUp() {
    //    Debug.Log("Mouse UP");
    //    if (dragging) {
    //        //GetComponent<Rigidbody>().isKinematic = false;
    //        // Get closest square & try to move the piece to it
    //        Square closest_square = board.getClosestSquare(transform.position);

    //        transform.position = previousPosition;

    //        this_piece.movePiece(closest_square);

    //        if (board.use_hover) board.resetHoveredSquares();
    //        dragging = false; // Stop dragging
    //    }
    //}
}