using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using solace;

public class Unit : MonoBehaviour {
    public string UnitName { get; set; }
    private int PrevMovement { get; set; }
    public bool IsSelected { get; set; }
    public string unitType { get; set; }
    public Character unitInfo;
    public Player LocalPlayer;
    private GameManager gmObject;
    public Tile CurrentLocation;
    private Renderer rend;
    private Color SaveColorOnMouseover;
    public Text info;
    public Vector3 heaven;

    

    // Use this for initialization
    void Start() {
        gmObject = GameObject.Find("Gameboard").GetComponent<GameManager>();
        LocalPlayer = GameObject.Find("Player").GetComponent<Player>();
        rend = GetComponent<Renderer>();
        heaven = new Vector3(0,0,9999);
        IsSelected = false;
    }

    // Update is called once per frame
    void Update()
    {

        //Unit is highlighted by mouseover
        if (rend.material.color == Color.red)
        {
            if (Input.GetMouseButtonDown(0))
            {
                CalculateMovement(false);
                IsSelected = false;
                rend.material.color = Color.white;
                SaveColorOnMouseover = Color.white;
                LocalPlayer.CurrentTarget = null;
            }
        }
        else if (rend.material.color == Color.grey)
        {
            if (Input.GetMouseButtonDown(0))
            {
                IsSelected = true;
                SaveColorOnMouseover = Color.red;
                LocalPlayer.CurrentTarget = this;
                CalculateMovement(true);
            }
        }

        //UH OH!!! You're dead.
        else if (unitInfo.CurrentHealth <= 0)
        {

            this.transform.position = heaven;
            CurrentLocation.currentUnit = null;
            CurrentLocation = null;
            unitInfo.CurrentHealth = 10;
        }
    }

    void OnMouseEnter()
    {
        SaveColorOnMouseover = rend.material.color;
        rend.material.color = Color.grey;

        int tempX = (int)Math.Round(transform.position.x);
        int tempY = (int)Math.Round(transform.position.z);
        CurrentLocation = gmObject.Cells[tempX][tempY];
        CurrentLocation.currentUnit = this;

        info.text = "--GENERAL INFO--\nUnit at location: x:" + transform.localPosition.x + ", y:" + transform.localPosition.z + ", z:" + transform.localPosition.y
            + "\nUnit Name: " + UnitName + "\nMovement: " + unitInfo.Movement + "\nHP: "
            + unitInfo.CurrentHealth + "/" + unitInfo.MaxHealth;

        List<string> keyList = new List<string>(unitInfo.coreDict.Keys);

        for (int i = 0; i < keyList.Count; i++)
        {
            List<string> innerKeyList = new List<string>(unitInfo.coreDict[keyList[i]].Keys);
            info.text += "\n--" + keyList[i].ToUpper() + "--";
            for (int j = 0; j < innerKeyList.Count; j++)
            {
                info.text += "\n" + innerKeyList[j] + ": " + unitInfo.coreDict[keyList[i]][innerKeyList[j]];
            }
        }
    }

    void OnMouseExit()
    {
        rend.material.color = SaveColorOnMouseover;
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
            {
                IsSelected = true;
                SaveColorOnMouseover = Color.red;
                LocalPlayer.CurrentTarget = this;
                CalculateMovement(true);
                
            }
    }
    
    //Move the unit to a new location
    public void MoveLocation(int newX, int newY)
    {
        string message = UnitName;
        Tile target = gmObject.Cells[newX][newY];

        if (target.IsEmpty())
        {
            if (IsValidMove(unitInfo.Movement, target.x, target.y))
            {
                //remove this unit from it's current tile
                CurrentLocation.currentUnit = null;
                CalculateMovement(false);
                //move it to a new position
                transform.position = (new Vector3(newX, 0.9f, newY));
                CurrentLocation = target;
                //set that positions owner unit to this
                target.currentUnit = this;

                message += " moved to new location: " + newX + "," + newY;
            }
            else
            {
                message += " can't move to that location. It is out of range!";
            }
        }
        else
        {
            //target is not empty!
            message += " can't move to new location. It is occupied or blocked. ";
        }
        Debug.Log(message);
    }

    //Determine the full range of movement
    public void CalculateMovement(bool highlight)
    {
        //save refference to the current location
        int currentX = CurrentLocation.x;
        int currentY = CurrentLocation.y;

        //save the value of movement when this is executed incase it changes while surrounding tiles are highlighted
        if (highlight)
        {
            PrevMovement = unitInfo.Movement;
            CMRecursive(unitInfo.Movement, true, currentX, currentY);
        }
        else
        {
            CMRecursive(PrevMovement, false, currentX, currentY);
        }
    }

    //starts the recursive chain to hightlight all available movement squares.
    private void CMRecursive(int move, bool highlight, int x, int y)
    {
        if (move > 0)
        {
            if (highlight)
            {
                //highlight the 4 'adjacent' cells, and call recursively on them
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.green);
                    CMRecursiveBottomRight(move - 1, highlight, x + 1, y);
                }
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.green);
                    CMRecursiveTopLeft(move - 1, highlight, x - 1, y);
                }
                if (gmObject.Cells[0].Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.green);
                    CMRecursiveTopRight(move - 1, highlight, x, y + 1);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.green);
                    CMRecursiveBottomLeft(move - 1, highlight, x, y - 1);
                }
            }
            else
            {
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.white);
                    CMRecursiveBottomRight(move - 1, highlight, x + 1, y);
                }
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.white);
                    CMRecursiveTopLeft(move - 1, highlight, x - 1, y);
                }
                if (gmObject.Cells[0].Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.white);
                    CMRecursiveTopRight(move - 1, highlight, x, y + 1);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.white);
                    CMRecursiveBottomLeft(move - 1, highlight, x, y - 1);
                }
            }
        }
        else
            return;

    }
    private void CMRecursiveTopLeft(int move, bool highlight, int x, int y)
    {
        if (move > 0)
        {
            if (highlight)
            {
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.green);
                    CMRecursiveTopLeft(move - 1, highlight, x - 1, y);
                }
                if (gmObject.Cells.Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.green);
                    CMRecursiveTopLeft(move - 1, highlight, x, y + 1);
                }
            }
            else
            {
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.white);
                    CMRecursiveTopLeft(move - 1, highlight, x - 1, y);
                }
                if (gmObject.Cells.Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.white);
                    CMRecursiveTopLeft(move - 1, highlight, x, y + 1);
                }
            }
        }
        else
            return;
    }
    private void CMRecursiveTopRight(int move, bool highlight, int x, int y)
    {
        if (move > 0)
        {
            if (highlight)
            {
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.green);
                    CMRecursiveTopRight(move - 1, highlight, x + 1, y);
                }
                if (gmObject.Cells[0].Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.green);
                    CMRecursiveTopRight(move - 1, highlight, x, y + 1);
                }
            }
            else
            {
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.white);
                    CMRecursiveTopRight(move - 1, highlight, x + 1, y);
                }
                if (gmObject.Cells[0].Count > y + 1)
                {
                    HighlightColor(gmObject.Cells[x][y + 1], Color.white);
                    CMRecursiveTopRight(move - 1, highlight, x, y + 1);
                }
            }
        }
        else
            return;
    }
    private void CMRecursiveBottomLeft(int move, bool highlight, int x, int y)
    {
        if (move > 0)
        {
            if (highlight)
            {
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.green);
                    CMRecursiveBottomLeft(move - 1, highlight, x - 1, y);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.green);
                    CMRecursiveBottomLeft(move - 1, highlight, x, y - 1);
                }
            }
            else
            {
                if (x - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x - 1][y], Color.white);
                    CMRecursiveBottomLeft(move - 1, highlight, x - 1, y);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.white);
                    CMRecursiveBottomLeft(move - 1, highlight, x, y - 1);
                }
            }
        }
        else
            return;
    }
    private void CMRecursiveBottomRight(int move, bool highlight, int x, int y)
    {
        if (move > 0)
        {
            if (highlight)
            {
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.green);
                    CMRecursiveBottomRight(move - 1, highlight, x + 1, y);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.green);
                    CMRecursiveBottomRight(move - 1, highlight, x, y - 1);
                }
            }
            else
            {
                if (gmObject.Cells.Count > x + 1)
                {
                    HighlightColor(gmObject.Cells[x + 1][y], Color.white);
                    CMRecursiveBottomRight(move - 1, highlight, x + 1, y);
                }
                if (y - 1 >= 0)
                {
                    HighlightColor(gmObject.Cells[x][y - 1], Color.white);
                    CMRecursiveBottomRight(move - 1, highlight, x, y - 1);
                }
            }
        }
        else
            return;
    }

    //helper to change the color of a tile
    private void HighlightColor(Tile target, Color newColor)
    {
        if(target.IsEmpty())
            (target.GetComponent<Renderer>()).material.color = newColor;
    }

    //Recursive method to establish if a target is a valid move for a unit
    public bool IsValidMove(int move, int newX, int newY)
    {
        //base case, location is a valid movement
        if (newX == CurrentLocation.x && newY == CurrentLocation.y)
            return true;

        if (move > 0)
        {
            //Move closer in the X direction
            if (newX > CurrentLocation.x)
            {
                return IsValidMove(move - 1, newX - 1, newY);
            }
            else if (newX < CurrentLocation.x)
            {
                return IsValidMove(move - 1, newX + 1, newY);
            }

            //Move closer in the Y direction
            else if (newY > CurrentLocation.y)
            {
                return IsValidMove(move - 1, newX, newY - 1);
            }
            else if (newY < CurrentLocation.y)
            {
                return IsValidMove(move - 1, newX, newY + 1);
            }
        }

        //ran out of moves, haven't reached the location, false!
        return false;
    }
}
