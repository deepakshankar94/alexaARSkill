using UnityEngine;

/*
==============================
[Coordinate] - Board "local" coordinate
==============================
*/
[System.Serializable]
public class Coordinate {
    public int x;
    public int y;
    public Vector3 pos; // Transform position in the scene

    public Coordinate(int x, int y) {
        this.x = x;
        this.y = y;
        pos = new Vector3(0, 0, 0);
    }

    public string GetBoardPosition()
    {
        string pos = "";
        switch (y)
        {
            case 0:
                pos = "a";
                break;
            case 1:
                pos = "b";
                break;
            case 2:
                pos = "c";
                break;
            case 3:
                pos = "d";
                break;
            case 4:
                pos = "e";
                break;
            case 5:
                pos = "f";
                break;
            case 6:
                pos = "g";
                break;
            case 7:
                pos = "h";
                break;
            default:
                break;
        }
        return pos + (x + 1);
    }

    public static Coordinate GetCoordinatePosition(string boardPos)
    {
        char[] pos = boardPos.ToCharArray();
        int x = -1, y = int.Parse(pos[1].ToString()) - 1;
        switch (pos[0])
        {
            case 'a':
                x = 0;
                break;
            case 'b':
                x = 1;
                break;
            case 'c':
                x = 2;
                break;
            case 'd':
                x = 3;
                break;
            case 'e':
                x = 4;
                break;
            case 'f':
                x = 5;
                break;
            case 'g':
                x = 6;
                break;
            case 'h':
                x = 7;
                break;
            default:
                break;
        }
        Debug.Log(x + " " + y);
        return new Coordinate(y, x);
    }

    public int GetSquareListIndex()
    {
        return (x * 8 + y);
    }
}