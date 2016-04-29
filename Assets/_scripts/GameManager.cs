using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using solace;

public class GameManager : MonoBehaviour
{
    //variables
    private int BoardSize;
    public Tile SquareOne;
    public Text output;
    public Unit Adam;
    public bool GameIsServer;
    public bool ClientIsOpen;

    //Core data structures for the game
    public List<Player> Players;
    public List<List<Tile>> Cells;
    public Dictionary<string, Unit> Units;
    public Dictionary<string, ActionSpell> Spells;
    public Dictionary<string, Character> UnitTypes;


    // Use this for initialization
    void Start()
    {
        BoardSize = 0;
        Players = new List<Player>();
        Cells = new List<List<Tile>>();
        Units = new Dictionary<string, Unit>();
        Spells = new Dictionary<string, ActionSpell>();
        UnitTypes = new Dictionary<string, Character>();
        LoadMods MLoader = new LoadMods(this);

        GameIsServer = false;
        ClientIsOpen = false;
        CreateBoard(10);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Create the initial board
    public void CreateBoard(int SizeOfBoard)
    {
        Units["adam"] = Adam;
        Adam.UnitName = "adam";
        Adam.unitInfo = UnitTypes["peon"];
        

        for (int x = 0; x < SizeOfBoard; ++x)
        {
            List<Tile> Column = new List<Tile>();
            for (int y = 0; y < SizeOfBoard; ++y)
            {
                if (x == 0 && y == 0)
                {
                    Column.Add(SquareOne);
                }
                else
                {
                    Tile newTile = (Tile)(Instantiate(SquareOne, new Vector3(x, 0, y), new Quaternion()));
                    newTile.x = x;
                    newTile.y = y;
                    Column.Add(newTile);
                }
            }
            Cells.Add(Column);
            ++BoardSize;
        }

        //put our first unit on the board
        Cells[0][0].currentUnit = Adam;
        Adam.CurrentLocation = Cells[0][0];
    }

    //Increase the board size
    public void IncreaseBoardSize(int SizeToIncrease)
    {
        //number of times to increase the size
        for (int i = 0; i < SizeToIncrease; ++i)
        {

            //add 1 to each existing row length
            for (int x = 0; x < BoardSize; ++x)
            {
                Tile newTile = (Tile)(Instantiate(SquareOne, new Vector3(x, 0, BoardSize), new Quaternion()));
                newTile.x = x;
                newTile.y = BoardSize;
                Cells[x].Add( newTile );
            }

            //add the new column on the side
            List<Tile> Row = new List<Tile>();
            for (int y = 0; y < BoardSize + 1; ++y)
            {
                Tile newTile = (Tile)(Instantiate(SquareOne, new Vector3(BoardSize, 0, y), new Quaternion()));
                newTile.x = BoardSize;
                newTile.y = y;
                Row.Add( newTile );
            }
            Cells.Add(Row);
            ++BoardSize;
        }
    }

    //Deacrease the board size if possible, else, return false
    public bool DecreaseBoardSize(int SizeToDecrease)
    {
        if (SizeToDecrease < BoardSize)
        {
            //until we are done
            for (int i = 0; i < SizeToDecrease; ++i)
            {
                //check the top row
                for (int j = 0; j < BoardSize; j++)
                {
                    if (!Cells[j][BoardSize - 1].IsEmpty())
                    {
                        output.text += "\n" + "Some spaces were occupied. Could not complete action.";
                        return false;
                    }
                }

                //check the outside column
                foreach (Tile tile in Cells[BoardSize - 1])
                {
                    if (!tile.IsEmpty())
                    {
                        output.text += "\n" + "Some spaces were occupied. Could not complete action.";
                        return false;
                    }
                }

                //delete the outside column
                for (int j = BoardSize - 1; j >= 0; j--)
                {
                    //remove and destroy all tiles
                    Tile temp = Cells[BoardSize - 1][j];
                    Cells[BoardSize - 1].Remove(temp);
                    Destroy(temp.gameObject);

                    //remove the list from the group
                }
                Cells.Remove(Cells[BoardSize - 1]);

                //check the top row
                for (int j = 0; j < BoardSize - 1; j++)
                {
                    //remove and destroy each tile at the end of the lists
                    Tile temp = Cells[j][BoardSize - 1];
                    Cells[j].Remove(temp);
                    Destroy(temp.gameObject);
                }
                BoardSize--;
            }
            output.text += "\n" + "Successfully removed all tiles.";
            return true;
        }
        else
            return false;
    }

    //console command for casting spells
    public void CastSpell(String[] tokens)
    {
        if (tokens.Length < 4)
        {
            output.text += "\n" + "Invalid number of arguments for <cast>. Expected 4, got " + tokens.Length 
                + ".\nUsage: cast <spell> <caster> <target>";
        }
        else
        {
            try
            {
                //get the componenets
                Unit caster = Units[tokens[2]];
                Unit target = Units[tokens[3]];
                ActionSpell spell = Spells[tokens[1]];

                //RANGE CHECK
                if(caster.IsValidMove(spell.spellRange, target.CurrentLocation.x, target.CurrentLocation.y))
                {
                    //find which attribute the spell is targeting
                    switch (spell.spellTargetAttribute)
                    {
                        case "health":
                            if(spell.spellOp == "sub")
                                target.unitInfo.CurrentHealth -= spell.spellValue;
                            else if (spell.spellOp == "add")
                                target.unitInfo.CurrentHealth += spell.spellValue;
                            else if (spell.spellOp == "div")
                                target.unitInfo.CurrentHealth /= spell.spellValue;
                            else if (spell.spellOp == "multi")
                                target.unitInfo.CurrentHealth *= spell.spellValue;
                            break;

                        case "movement":
                            if (spell.spellOp == "sub")
                                target.unitInfo.Movement -= spell.spellValue;
                            else if (spell.spellOp == "add")
                                target.unitInfo.Movement += spell.spellValue;
                            else if (spell.spellOp == "div")
                                target.unitInfo.Movement /= spell.spellValue;
                            else if (spell.spellOp == "multi")
                                target.unitInfo.Movement *= spell.spellValue;
                            break;
                        
                        //the hard part. Dig through all the modable fields to find one we are looking for
                        default:
                            List<string> keyList = new List<string>(target.unitInfo.coreDict.Keys);
                            
                            for (int i = 0; i < keyList.Count; i++)
                            {
                                List<string> innerKeyList = new List<string>(target.unitInfo.coreDict[keyList[i]].Keys);
                                for (int j = 0; j < innerKeyList.Count; j++)
                                {
                                    if (innerKeyList[j] == spell.spellTargetAttribute)
                                    {
                                        if (spell.spellOp == "sub")
                                            target.unitInfo.coreDict[keyList[i]][innerKeyList[j]] -= spell.spellValue;
                                        else if (spell.spellOp == "add")
                                            target.unitInfo.coreDict[keyList[i]][innerKeyList[j]] += spell.spellValue;
                                        else if (spell.spellOp == "div")
                                            target.unitInfo.coreDict[keyList[i]][innerKeyList[j]] /= spell.spellValue;
                                        else if (spell.spellOp == "multi")
                                            target.unitInfo.coreDict[keyList[i]][innerKeyList[j]] *= spell.spellValue;
                                        output.text += "\n" + "Cast successful.";
                                        i = keyList.Count;
                                        j = innerKeyList.Count;
                                    }
                                }
                                if (i + 1 == keyList.Count)
                                    output.text += "\n" + "Could not find the affected attribute on target. Cast failed.";
                            }
                            break;
                    }
                }
                else
                {
                    output.text += "\n" + "The target is out of range.";
                }
            }
            catch (Exception e)
            {
                output.text += "\n" + "Caster, Target, or spell name incorrect. Check names." + e;
            }
        }
    }

    //create a unit
    public void CreateUnit(String[] tokens)
    {
        ////no specified location, start at origin and search for an empty space.
        //if (tokens.Length == 3)
        //{
        //    int x = 0, y = 0;
        //    //Find an empty space to instantiate a player.
        //    for(y = 0; y < BoardSize - 1; y++)
        //    {
        //        for(x = 0; x < BoardSize - 1; x++)
        //        {
        //            if (Cells[x][y].IsEmpty())
        //            {
        //                break;
        //            }
        //        }
        //        if (Cells[x][y].IsEmpty())
        //        {
        //            break;
        //        }
        //    }

        //    //if board is full
        //    if (x == Cells.Count - 1 && y == Cells.Count - 1)
        //    {
        //        output.text += "\n" + "There are no available spaces on the board to spawn a unit.";
        //    }
        //    //place unit on the empty space
        //    else
        //    {
        //        Unit newUnit = (Unit)(Instantiate(Adam, new Vector3(x, 0.9f, y), new Quaternion()));
        //        newUnit.UnitName = tokens[2];
        //        Cells[x][y].currentUnit = newUnit;
        //        Units.Add(newUnit);

        //        output.text += "\n" + "New Unit " + tokens[2] + " created at location " + x + "," + y;
        //    }
        //}

        //location is specifed
        if (tokens.Length == 4)
        {
            //if unit can be found, then it already exists, and needs a different name
            try
            {
                Unit newUnit = Units[tokens[2].ToLower()];
                output.text += "\n" + "There is already a unit of that name.\nUnit names must be unique and are not case sensitive.";
            }
            //if the unit does not exist, it will trigger a KeyNotFoundException
            catch 
            {
                // Break up 3,4 into x an y
                string[] temp = tokens[3].Split(',');
                int x = int.Parse(temp[0]);
                int y = int.Parse(temp[1]);

                //Make sure the x and y are in range
                if (x < Cells.Count && y < Cells.Count)
                {
                    //if the space is empty, place the unit there
                    if (Cells[x][y].IsEmpty())
                    {
                        //instantiate the new unit
                        Unit newUnit = (Unit)(Instantiate(Adam, new Vector3(x, 0.9f, y), new Quaternion()));

                        try
                        {
                            //copy the premade type so that we can have our own data
                            Character copy = new Character(UnitTypes[tokens[1]]);
                            newUnit.unitInfo = copy;
                        }
                        catch
                        {
                            output.text += "\n" + "Couldnt find the type you specifed.";
                        }

                        //name it and add it to the list of all units
                        newUnit.UnitName = tokens[2].ToLower();
                        newUnit.CurrentLocation = Cells[x][y];
                        Units[newUnit.UnitName] = newUnit;

                        //place the unit on the virtual board
                        Cells[x][y].currentUnit = newUnit;
                        output.text += "\n" + "New Unit " + tokens[2] + " created at location " + x + "," + y;
                    }
                    else
                    {
                        output.text += "\n" + "The specified location is occupied.";
                    }
                }
                else
                {
                    output.text += "\n" + "The specified location is off the board.";
                }
            }
          
        }
        else
        {
            output.text += "\n" + "Wrong number of arguments."
                + "\n" + "Usage: create <type> <name> <location in format x,y>";
        }
    }
}