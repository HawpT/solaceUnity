using System.Collections.Generic;
using System;

namespace solace
{
    public class Character
    {
        //Dictionary of Dictionaries, keys are string names of each category of attribute
        public Dictionary<string, Dictionary<string, int>> coreDict;
        public string unitType { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int Movement { get; set; }


        //default constuctor
        public Character()
        {
            coreDict = new Dictionary<string, Dictionary<string, int>>();
            MaxHealth = 10;
            CurrentHealth = 10;
            unitType = "Basic";
            Movement = 3;
        }

        public Character(Character copy)
        {
            coreDict = copy.coreDict;
            MaxHealth = copy.MaxHealth;
            CurrentHealth = copy.CurrentHealth;
            unitType = copy.unitType;
            Movement = copy.Movement;
        }
    }
}
