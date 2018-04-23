using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
==============================
[Piece] - Script placed on every piece in the board.
==============================
*/
public class Piece : MonoBehaviour {

    public string name;
    public Animator animator;
    public Material billboard;

    bool isEating = false;
    Piece pieceToEat;

    private List<Move> allowed_moves = new List<Move>(); // List of moves the piece can move, starting from its current position
    private MoveType move_type; // Type of move the piece is going to move
    private Piece castling_tower; // Once we know which tower the king is trying to castle, we save it here

    public List<Coordinate> break_points = new List<Coordinate>(); // Coordinates that will break the piece's direction
    public bool started; // StartOnly MoveType controller, set to true when the piece moves for the first time
    public Square cur_square; // Where is the piece right now
    public Board board;

    [SerializeField]
    public string piece_name;

    [SerializeField]
    public int team; // Whites = -1, Blacks = 1

    [SerializeField]
    public List<Piece> castling_towers;

    void Start() {
        // Initialize valid moves
        switch (piece_name) {
            case "Pawn":
                if (team == -1)
                {
                    if (cur_square.coor.x != 1)
                        started = true;
                }
                else
                {
                    if (cur_square.coor.x != 6)
                        started = true;
                }
                addPawnAllowedMoves();
                break;
            case "Tower":
                addLinealAllowedMoves();
                break;
            case "Horse":
                addHorseAllowedMoves();
                break;
            case "Bishop":
                addDiagonalAllowedMoves();
                break;
            case "Queen":
                addLinealAllowedMoves();
                addDiagonalAllowedMoves();
                break;
            case "King":
                addKingAllowedMoves();
                break;
        }
    }

    /*
    ---------------
    Moves related functions
    ---------------
    */ 
    // Once the user drops the piece, we'll try to move it, if it was dropped in a non-valid square,
    // the piece will be returned to its position
    public void movePiece(Square square, bool isLocal = true) {
        if (checkValidMove(square)) {
            // Switch cases for the current move type
            switch (move_type) {
                case MoveType.StartOnly:
                    // If the piece is the king and can castle
                    if (piece_name == "King" && checkCastling(square)) {
                        // Update castling tower's position (depending on where the tower is, we will move it 3 or 2 squares in the "x" axis)
                        if (castling_tower.cur_square.coor.y == 0) {
                            castling_tower.castleTower(castling_tower.cur_square.coor.y + 2);
                        }
                        else {
                            castling_tower.castleTower(castling_tower.cur_square.coor.y - 3);
                        }
                    }
                    break;
                case MoveType.Eat:
                case MoveType.EatMove:
                case MoveType.EatMoveJump:
                    // If the move type involves eating, eat the enemy piece
                    if (square.holding_piece != null)
                    {
                        isEating = true;
                        pieceToEat = square.holding_piece;
                    }
                    //eatPiece(square.holding_piece);
                    break;
            }
            if (isLocal)
                board.PieceMoved(this.name, cur_square, square);

            // Update piece's current square
            cur_square.holdPiece(null);
            square.holdPiece(this);
            cur_square = square;
            if (!started) started = true;

            // Change game's turn
            board.changeTurn();
            StartCoroutine(moveObject(transform.position, new Vector3(cur_square.coor.pos.x, transform.position.y, cur_square.coor.pos.z), 1.5f));
        }

        // Clear break points & update piece's position
        break_points.Clear();
        //transform.position = new Vector3(cur_square.coor.pos.x, transform.position.y, cur_square.coor.pos.z);
        //transform.rotation = new Quaternion(0, 0, 0, 0);
        //StartCoroutine(MoveFromTo(transform.position, new Vector3(cur_square.coor.pos.x, transform.position.y, cur_square.coor.pos.z), 0.025f));
        //StartCoroutine(moveObject(transform.position, new Vector3(cur_square.coor.pos.x, transform.position.y, cur_square.coor.pos.z), 1.5f));
    }

    IEnumerator moveObject(Vector3 source, Vector3 target, float duration)
    {
        billboard.color = new Color(1, 1, 1, 0.5f);
        Debug.Log("Actual target " + target);
        if (isEating)
        {
            StartCoroutine(Kill(duration, target));
            target -= new Vector3(0f, 0f, 1f) * team * board.transform.lossyScale.x;
            duration *= 0.75f;
            Debug.Log("Kill move");
        }
        Debug.Log("Started walking " + target.x + " " + target.y + " " + target.z);

        animator.SetBool("isWalking", true);
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / duration);
            yield return null;
        }
        transform.position = target;
        animator.SetBool("isWalking", false);
        Debug.Log("Done walking");
        billboard.color = new Color(1, 1, 1, 1);
        board.DisplayHiddenPieces();
    }

    IEnumerator Kill(float duration, Vector3 target)
    {
        Debug.Log("Waiting to kill " + duration + " " + Time.time);
        yield return new WaitForSeconds(duration * 0.75f);

        Debug.Log("Can eat");
        isEating = false;
        animator.SetTrigger("Kill");
        eatPiece(pieceToEat);
        pieceToEat = null;
        Debug.Log("Killed " + Time.time);

        yield return new WaitForSeconds(0.6f);

        float startTime = Time.time;
        Vector3 source = transform.position;
        Debug.Log("Moving from " + source + " to " + target);
        animator.SetBool("isWalking", true);
        while (Time.time < startTime + (duration * 0.25f))
        {
            transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / (duration * 0.25f));
            yield return null;
        }
        transform.position = target;
        animator.SetBool("isWalking", false);
    }

    IEnumerator MoveFromTo(Vector3 a, Vector3 b, float speed)
    {
        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        Debug.LogFormat("{0} {1}", a, b);
        float t = 0;
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            transform.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        transform.position = b;
        board.DisplayHiddenPieces();
    }

    // Get the coordinate starting from this piece position (0, 0)
    public Coordinate getCoordinateMove(Square square) { 
        int coor_x = (square.coor.x - cur_square.coor.x) * team;
        int coor_y = (square.coor.y - cur_square.coor.y) * team;

        return new Coordinate(coor_x, coor_y);
    }

    // Check if the piece can move to the given square
    public bool checkValidMove(Square square) {
        Coordinate coor_move = getCoordinateMove(square);

        for (int i = 0; i < allowed_moves.Count ; i++) {
            if (coor_move.x == allowed_moves[i].x && coor_move.y == allowed_moves[i].y) {
                move_type = allowed_moves[i].type;
                switch (move_type) {
                    case MoveType.StartOnly:
                        // If this piece hasn't been moved before, can move to the square or is trying to castle
                        if (!started && checkCanMove(square) && checkCastling(square)) 
                            return true;
                        break;
                    case MoveType.Move:
                        if (checkCanMove(square)) {
                            return true;
                        } 
                        break;
                    case MoveType.Eat:
                        if (checkCanEat(square)) 
                            return true;
                        break;
                    case MoveType.EatMove:
                    case MoveType.EatMoveJump:
                        if (checkCanEatMove(square)) {
                            return true;
                        }
                        break;
                }
            }
        }
        return false;
    }

    // Check if we move this piece to the given square the king keeps in check mode
    public bool checkValidCheckKingMove(Square square) {
        bool avoids_check = false;

        Piece old_holding_piece = square.holding_piece;
        Square old_square = cur_square;
        
        cur_square.holdPiece(null);
        cur_square = square;
        square.holdPiece(this);

        // If my king isn't checked or I can eat the checking piece
        if (!board.isCheckKing(board.cur_turn) || (square == board.checking_pieces[team].cur_square)) {
            avoids_check =  true;
        }

        cur_square = old_square;
        cur_square.holdPiece(this);
        square.holdPiece(old_holding_piece);
        return avoids_check;
    }

    // Returns if the piece can move to the given square
    private bool checkCanMove(Square square) {
        Coordinate coor_move = getCoordinateMove(square);

        // If square is free, square isn't further away from the breaking squares and the move won't cause a check
        if (square.holding_piece == null && checkBreakPoint(coor_move) && checkValidCheckKingMove(square)) return true;
        return false;
    }

    // Returns if the piece can eat an enemy piece that is placed in the given square
    private bool checkCanEat(Square square) {
        Coordinate coor_move = getCoordinateMove(square);

        // If square is holding an enemy piece, square isn't further away from the breaking squares and the move won't cause a check
        if (square.holding_piece != null && square.holding_piece.team != team && checkBreakPoint(coor_move) && checkValidCheckKingMove(square)) return true;
        return false;
    }

    // Returns if the piece can eat or move to the given square
    private bool checkCanEatMove(Square square) {
        if (checkCanEat(square) || checkCanMove(square)) return true; 
        return false;
    }

    /*
    ---------------
    Break points related functions
    ---------------
    */ 
    // Checks if the given coordinate isn't far away from the breaking points.
    // Since the given coordinate is related to the current square's position,
    // we'll need to check all the axis possibilities (negatives and positives)
    private bool checkBreakPoint(Coordinate coor) {
        for (int i = 0; i < break_points.Count; i++) {
            if (break_points[i].x == 0 && coor.x == 0){
                if (break_points[i].y < 0 && (coor.y < break_points[i].y)) {
                    return false;
                }
                else if (break_points[i].y > 0 && (coor.y > break_points[i].y)) {
                    return false;
                }
            }
            else if (break_points[i].y == 0 && coor.y == 0){
                if (break_points[i].x > 0 && (coor.x > break_points[i].x)) {
                    return false;
                }
                else if (break_points[i].x < 0 && (coor.x < break_points[i].x)) {
                    return false;
                }
            }
            else if (break_points[i].y > 0 && (coor.y > break_points[i].y)) {
                if (break_points[i].x > 0 && (coor.x > break_points[i].x)) {
                    return false;
                }
                else if (break_points[i].x < 0 && (coor.x < break_points[i].x)) {
                    return false;
                }
            }
            else if (break_points[i].y < 0 && (coor.y < break_points[i].y)){
                if (break_points[i].x > 0 && (coor.x > break_points[i].x)) {
                    return false;
                }
                else if (break_points[i].x < 0 && (coor.x < break_points[i].x)) {
                    return false;
                }
            }
        }
        return true;
    }

    // Add piece's break positions, squares that are further away won't be allowed
    public void addBreakPoint(Square square) {
        Coordinate coor_move = getCoordinateMove(square);

        for (int j = 0; j < allowed_moves.Count ; j++) {
            if (coor_move.x == allowed_moves[j].x && coor_move.y == allowed_moves[j].y) {
                switch (allowed_moves[j].type) {
                    case MoveType.StartOnly:
                    case MoveType.Move:
                    case MoveType.Eat:
                    case MoveType.EatMove:
                        // If square is holding a piece
                        if (square.holding_piece != null) {
                            break_points.Add(coor_move);
                        } 
                        break;
                }
            }
        }   
    }

    /*
    ---------------
    Castling related functions
    ---------------
    */ 
    // Castle this tower with the king, updating its position
    public void castleTower(int coor_y) {
        Coordinate castling_coor = new Coordinate(cur_square.coor.x,coor_y);
        Square square = board.getSquareFromCoordinate(castling_coor);

        cur_square.holdPiece(null);
        square.holdPiece(this);
        cur_square = square;
        if (!started) started = true;

        transform.position = new Vector3(cur_square.coor.pos.x, transform.position.y, cur_square.coor.pos.z);
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    // Check if the king can make a castle
    private bool checkCastling(Square square) {
        if (piece_name == "King") {
            float closest_castling = Vector3.Distance(square.coor.pos, castling_towers[0].transform.position);
            castling_tower = castling_towers[0];

            for (int i = 0; i < castling_towers.Count; i++) {
                if (Vector3.Distance(square.coor.pos, castling_towers[i].transform.position) <= closest_castling) {
                    castling_tower = castling_towers[i];
                }
            }
            bool can_castle = board.checkCastlingSquares(cur_square, castling_tower.cur_square, team);

            return (!castling_tower.started && can_castle) ? true : false;
        }  
        else {
            return true;
        }
    }

    // Adds an allowed piece move
    private void addAllowedMove(int coor_x, int coor_y, MoveType type) {
        Move new_move = new Move(coor_x, coor_y, type);
        allowed_moves.Add(new_move);
    }

    // Pawns allowed moves
    private void addPawnAllowedMoves()
    {
        addAllowedMove(-1, 0, MoveType.Move);
        addAllowedMove(-2, 0, MoveType.StartOnly);
        addAllowedMove(-1, 1, MoveType.Eat);
        addAllowedMove(-1, -1, MoveType.Eat);
    }

    // Towers & part of the Queen's alowed moves
    private void addLinealAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 8; coor_x++)
        {
            addAllowedMove(coor_x, 0, MoveType.EatMove);
            addAllowedMove(0, coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, 0, MoveType.EatMove);
            addAllowedMove(0, -coor_x, MoveType.EatMove);
        }
    }

    // Bishops & part of the Queen's alowed moves
    private void addDiagonalAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 8; coor_x++)
        {
            addAllowedMove(coor_x, -coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, coor_x, MoveType.EatMove);
            addAllowedMove(coor_x, coor_x, MoveType.EatMove);
            addAllowedMove(-coor_x, -coor_x, MoveType.EatMove);
        }
    }

    // Horses allowed moves
    private void addHorseAllowedMoves()
    {
        for (int coor_x = 1; coor_x < 3; coor_x++)
        {
            for (int coor_y = 1; coor_y < 3; coor_y++)
            {
                if (coor_y != coor_x)
                {
                    addAllowedMove(coor_x, coor_y, MoveType.EatMoveJump);
                    addAllowedMove(-coor_x, -coor_y, MoveType.EatMoveJump);
                    addAllowedMove(coor_x, -coor_y, MoveType.EatMoveJump);
                    addAllowedMove(-coor_x, coor_y, MoveType.EatMoveJump);
                }
            }
        }
    }

    // King's allowed moves (castling included)
    private void addKingAllowedMoves()
    {
        // Castling moves
        addAllowedMove(0, -2, MoveType.StartOnly);
        addAllowedMove(0, 2, MoveType.StartOnly);

        // Normal moves
        addAllowedMove(0, 1, MoveType.EatMove);
        addAllowedMove(-1, 1, MoveType.EatMove);
        addAllowedMove(-1, 0, MoveType.EatMove);
        addAllowedMove(-1, -1, MoveType.EatMove);
        addAllowedMove(0, -1, MoveType.EatMove);
        addAllowedMove(1, -1, MoveType.EatMove);
        addAllowedMove(1, 0, MoveType.EatMove);
        addAllowedMove(1, 1, MoveType.EatMove);
    }

    /*
    ---------------
    Other functions
    ---------------
    */
    public void setStartSquare(Square square) {
        cur_square = square;
    } 

    // Function called when someone eats this piece
    public void eatMe() {
        animator.SetTrigger("Die");
        StartCoroutine(cur_square.Die());
        board.destroyPiece(this);
        Destroy(this.gameObject, 0.5f);
    }

    // Called when this piece is eating an enemy piece
    private void eatPiece(Piece piece) {
        if (piece != null && piece.team != team) piece.eatMe();
    }
}