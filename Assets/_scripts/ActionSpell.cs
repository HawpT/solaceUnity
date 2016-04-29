
public class ActionSpell
{
    //class variables
    public int spellRange { get; set; }
    public int spellArea { get; set; }
    public int spellValue { get; set; }
    public string spellName { get; set; }
    public string spellOp { get; set; }

    //category and attribute being affected by the spell
    public string spellTargetAttribute { get; set; }
        

    //defaults to modifying health
    public ActionSpell()
    {
        spellRange = 1;
        spellArea = 1;
        spellValue = 1;
        spellName = "melee";
        spellTargetAttribute = "health";
        spellOp = "sub";
    }
}

