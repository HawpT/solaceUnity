using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Assets._scripts;

public class ConsoleInput : MonoBehaviour {
    private InputField consoleInput;
	private InputField.SubmitEvent se;
    private GameManager gmObject;
	public Text output;
    private Client client;

    // Use this for initialization
    void Start () {
		consoleInput = GameObject.Find("InputField").GetComponent<InputField>();
        gmObject = GameObject.Find("Gameboard").GetComponent<GameManager>();
        se = new InputField.SubmitEvent();

        se.AddListener(TextEntered);
        consoleInput.onEndEdit = se;
        client = new Client();
	}
    

    public void writeToConsole(String message)
    {
        output.text += "\n" + message;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp("enter"))
        {
            consoleInput.ActivateInputField();
        }
        if (gmObject.ClientIsOpen)
        {
            if (Client.messages.Count > 0)
            {
                Packet temp = Client.messages.Dequeue();
                ParseCommand(temp.newCommand,true);
                output.text += "\n" + temp.senderID + " >> " + temp.newCommand;
            }
        }

    }

    //captures text when it is entered in the console consoleInput field.
    void TextEntered(String newText)
    {
        Debug.Log("Console Input: " + newText);
        output.text += "\n>>" + newText;
        ParseCommand(newText,false);
        consoleInput.text = "";
    }

    void OnApplicationQuit()
    {
        if (gmObject.ClientIsOpen)
            Client.Disconnect();
        if (gmObject.GameIsServer)
            Server.DisconnectAll();
    }

    //parse the command entered on the console
    public void ParseCommand(String command , bool MessageIsFromServer)
    {

        if (output.text.Length > 2000)
            output.text = output.text.Substring(1800, output.text.Length - 1801);

        //break the input into tokens, delete leading and trailing whitespace. 
        if (!(command == "" || command == null))
        {
            command.Trim();
            String[] tokens;
            tokens = command.Split(default(String[]), StringSplitOptions.RemoveEmptyEntries);

            if (gmObject.ClientIsOpen && !MessageIsFromServer)
            {
                Client.SendCommand(command);
            }

            switch (tokens[0])
            {
                /*
                * ADD MORE CASES HERE TO 'LISTEN' FOR MORE ARGUMENTS
                */
                //casting a spell
                case "cast":

                    gmObject.CastSpell(tokens);
                    
                    break;

                //creating a character or something else
                case "create":

                    if (tokens.Length == 4)
                    {
                        gmObject.CreateUnit(tokens);
                    }
                    else
                    {
                        output.text += "\n" + "Wrong number of arguments.\nUsage: create <type> <name> <location in format x,y>";
                    }
                    break;

                case "board":
                    if (tokens.Length == 3) {
                        if (tokens[1] == "increase")
                        {
                            int temp;
                            if ((temp = int.Parse(tokens[2])) > 0)
                            {
                                gmObject.IncreaseBoardSize(temp);
                            }
                            else
                            {
                                output.text += "\n" + "Argument 2 (" + temp + ") must be a postitive integer";
                            }
                        }
                        else if (tokens[1] == "decrease")
                        {
                            int temp;
                            if ((temp = int.Parse(tokens[2])) > 0)
                            {
                                gmObject.DecreaseBoardSize(temp);
                            }
                            else
                            {
                                output.text += "\n" + "Argument 2 (" + temp + ") must be a postitive integer";
                            }
                        }
                        //add more else if statements for further arguments
                    }
                    else
                    {
                        output.text += "\n" + tokens.Length + " is the wrong number of arguments." + "\n" + "Usage: board <operation> <value>";
                    }
                    break;

                //move a unit or prop to a new location
                case "move":
                    //move takes 4 arguments
                    if (tokens.Length == 4)
                    {
                        //behavior for all Units
                        if (tokens[1] == "character" || tokens[1] == "npc" || tokens[1] == "pleb" || tokens[1] == "char" || tokens[1] == "unit")
                        {
                            try {
                                //attempt to find the unit in our list
                                Unit targetUnit = gmObject.Units[tokens[2].ToLower()];

                                //pull out the X and Y locations
                                string[] temp = tokens[3].Split(',');
                                int newX = int.Parse(temp[0]);
                                int newY = int.Parse(temp[1]);
                                if (targetUnit.IsValidMove(targetUnit.unitInfo.Movement, newX, newY))
                                {
                                    targetUnit.MoveLocation(newX, newY);
                                }
                                else
                                {
                                    output.text += "\n" + "The destination is out of range.";
                                }

                            }
                            catch (KeyNotFoundException e)
                            {
                                e.ToString();
                                output.text += "\n" + "A unit of name " + tokens[2] + " could not be found.";
                            }
                            catch(Exception e)
                            {
                                output.text += "\n" + "Could not find coordinates. Use format x,y\n" + e;
                            }
                        }
                        else
                        {
                            output.text += "\n" + "Move what? Could not recognize command.";
                        }
                    }
                    else
                    {
                        output.text += "\n" + "Wrong number of arguments.\nUsage: move <type> <name/id> <location>";
                    }
                    break;

                //act as server the server
                case "host":
                    if (gmObject.GameIsServer || gmObject.ClientIsOpen)
                    {
                        output.text += "\n" + "Cannot host a game. You are already hosting or connected to one."
                            + "\nPlease restart a game if you wish to be the host.";
                        break;
                    }

                    //save server address
                    try
                    {
                        if (tokens.Length == 3)
                        {
                            string[] ServerAddress = tokens[1].Split(':');
                            string ip = ServerAddress[0];
                            string port = ServerAddress[1];
                            string temp = Server.StartServer(ip, port);
                            string temp1 = Client.StartClient(ip, port, tokens[2]);
                            gmObject.GameIsServer = false;
                            gmObject.ClientIsOpen = true;
                            output.text += "\n" + temp;

                        }
                        if (tokens.Length == 2)
                        {
                            string ip = "127.0.0.1";
                            string port = "8000";
                            string temp = Server.StartServer(ip, port);
                            string temp1 = Client.StartClient(ip, port, tokens[1]);
                            gmObject.GameIsServer = false;
                            gmObject.ClientIsOpen = true;
                            output.text += "\n" + temp + " @default 127.0.0.1:8000";
                        }
                        //parsing error, wrong number of arguments
                        else
                        {
                            output.text += "\n" + "Wrong number of arguments.\nUsage: host <ip:port> <username>\nOR: host <username>";
                        }
                    }
                    catch
                    {
                        output.text += "\n" + "Could not generate server. Check IP or Port";
                    }
                    break;
                
                //connect to a server
                case "connect":
                    //check to see if game is already in client mode
                    if (gmObject.ClientIsOpen)
                    {
                        output.text += "\n" + "You are already connected to a game."
                            + "\nPlease restart the game to connect to a different server";
                        break;
                    }

                    try
                    {
                        //Change the game to client mode
                        if (tokens.Length == 3)
                        {
                            gmObject.ClientIsOpen = true;
                            string[] ServerAddress = tokens[1].Split(':');
                            string ip = ServerAddress[0];
                            string port = ServerAddress[1];

                            //attempt a connection
                            string temp = Client.StartClient(ip, port, tokens[2]);
                            if (temp == "Connected to server!")
                            {
                                output.text += "\n" + temp;
                            }
                        }
                        else if (tokens.Length == 2)
                        {
                            gmObject.ClientIsOpen = true;
                            string ip = "127.0.0.1";
                            string port = "8000";

                            //attempt a connection
                            string temp = Client.StartClient(ip, port, tokens[1]);
                            if (temp == "Connected to server!")
                            {
                                output.text += "\n" + temp + " @default 127.0.0.1:8000";
                            }
                        }
                        else
                        {
                            output.text += "\n" + "Wrong number of arguments.\nUsage: connect <ip:port> <username>"
                                + "\nOR: host <username>";
                        }
                    }
                    catch
                    {
                        output.text += "\n" + "Could not star client. Check IP or Port";
                    }
                    break;

                //cleanly disconnect from the server
                case "disconnect":
                    if (gmObject.ClientIsOpen)
                    {
                        Client.Disconnect();
                        if (gmObject.GameIsServer)
                        {
                            Server.DisconnectAll();
                        }
                    }
                    gmObject.ClientIsOpen = false;
                    gmObject.GameIsServer = false;
                    output.text += "\n" + "Disconnecting from any open connection.";
                    break;
                    
                default:
                    break;
            }
        }
    }
    

    
}
