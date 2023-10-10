using System;

[Serializable]
public class Buffs 
{
    public string cardname;
    public int buffDP;
    public bool isJamming;
    public bool isBlock;
    public bool breakEvo;//진화원 파기 입니까?
    public bool isFix;//고정값을 줍니까? false이면 고정값이 아닌 더한값
    public int securityAttack;

    public int removeEvoCount;//지울 카드가 -1이면 진화원 전부 나머지는 개수대로
    public int buffTurn = 1;
}
