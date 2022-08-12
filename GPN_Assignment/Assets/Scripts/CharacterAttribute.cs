using System;

[Serializable]
public class CharacterAttribute
{
    //Attribute Stats
    public int strength;
    public int defense;
    public int health;
    public int mana;
    public int level;
    public int experience;

    //Resources
    public int gold;

    //Stats Pt
    public int strengthStatsPt;
    public int defenseStatsPt;
    public int healthStatsPt;
    public int remainingStatsPt;

    public CharacterAttribute()
    {

    }
    public CharacterAttribute(int strength, int defense, int health,int mana, int level, int experience, int gold, int strengthPt, int defensePt, int healthPt, int remainingPt)
    {
        this.strength = strength;
        this.defense = defense;
        this.health = health;
        this.mana = mana;
        this.level = level;
        this.experience = experience;
        this.gold = gold;
        strengthStatsPt = strengthPt;
        defenseStatsPt = defensePt;
        healthStatsPt = healthPt;
        remainingStatsPt = remainingPt;
    }
}
