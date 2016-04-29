using System.IO;
using System.Xml;
using System.Collections.Generic;
using solace;

public class LoadMods
{
    private string[] spellMods;
    private string[] unitMods;
    public GameManager gmObject;
    private XmlTextReader reader;
    

    public LoadMods(GameManager newGM)
    {
        gmObject = newGM;
        spellMods = Directory.GetFiles("Assets/_scripts/mods/spells", "*.xml");
        unitMods = Directory.GetFiles("Assets/_scripts/mods/unit", "*.xml");
        loadSpellMods();
        loadUnitMods();
    }
    

    private void loadSpellMods()
    {
        
        for (int i = 0; i < spellMods.Length; i++)
        {
            reader = new XmlTextReader(spellMods[i]);
            string temp = "";
            ActionSpell newSpell = new ActionSpell();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        temp = reader.Name;
                        break;

                    case XmlNodeType.Text: //Display the text in each element.
                        SpellHelper(temp, reader.Value, newSpell);
                        break;

                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;

                    default:
                        break;
                }
            }
            gmObject.Spells[newSpell.spellName] = newSpell;
            reader.Close();
        }
    }

    //Helper method to de-cluter our XML reader method
    private void SpellHelper(string tag, string value, ActionSpell spell)
    {
        
        switch(tag){
            case "name":
                spell.spellName = value;
                break;

            case "area":
                spell.spellArea = int.Parse(value);
                break;

            case "range":
                spell.spellRange = int.Parse(value);
                break;

            case "modTarget":
                spell.spellTargetAttribute = value;
                break;

            case "modAmount":
                spell.spellValue = int.Parse(value);
                break;

            case "modOperation":
                spell.spellOp = value;
                break;

            default:
                break;
        }
    }

    private void loadUnitMods()
    {
        for (int i = 0; i < unitMods.Length; i++)
        {
            reader = new XmlTextReader(unitMods[i]);
            string temp = "";
            string type = "";
            Character newUnit = new Character();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        temp = reader.Name;
                        if(reader.AttributeCount > 0)
                            type = reader.GetAttribute(0);
                        break;

                    case XmlNodeType.Text: //Display the text in each element.
                        UnitHelper(temp, type, reader.Value, newUnit);
                        break;

                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;

                    default:
                        break;
                }
            }
            reader.Close();
            gmObject.UnitTypes[newUnit.unitType] = newUnit;
        }
    }

    private void UnitHelper(string tag, string category, string value, Character unit)
    {
        switch (tag)
        {
            case "type":
                unit.unitType = value;
                break;

            case "health":
                unit.MaxHealth = int.Parse(value);
                unit.CurrentHealth = unit.MaxHealth;
                break;

            case "movement":
                unit.Movement = int.Parse(value);
                break;

            
            //Default behavior will be for dictionary elements
            default:
                try
                {
                    unit.coreDict[category][tag] = int.Parse(value);
                }
                catch
                {
                    unit.coreDict[category] = new Dictionary<string, int>();
                    unit.coreDict[category][tag] = int.Parse(value);
                }
                break;
        }
    }
}
