using System;

[Serializable]
public class Buffs 
{
    public string cardname;
    public int buffDP;
    public bool isJamming;
    public bool isBlock;
    public bool breakEvo;//��ȭ�� �ı� �Դϱ�?
    public int securityAttack;

    public int removeEvoCount;//���� ī�尡 -1�̸� ��ȭ�� ���� �������� �������
    public int buffTurn = 1;
}
