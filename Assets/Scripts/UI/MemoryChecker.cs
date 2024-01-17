using UnityEngine;
using Mirror;

public class MemoryChecker : NetworkBehaviour
{
    public static MemoryChecker Inst { get; private set; }
    void Awake() => Inst = this;

    [SyncVar]
    public int memory;

    [SyncVar]
    public int buffMemory;
    [SyncVar]
    public int instantMemory;

    public void memoryCheckerPos()
    {
        //if (Player.localPlayer != null && Player.localPlayer.isLocalPlayer)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (Player.localPlayer.firstPlayer)
            {
                switch (memory)
                {
                    case 0:
                        rectTransform.anchoredPosition = new Vector2(0, 32);
                        break;
                    case 1:
                        rectTransform.anchoredPosition = new Vector2(-92, 32);
                        break;
                    case 2:
                        rectTransform.anchoredPosition = new Vector2(-92 * 2, 32);
                        break;
                    case 3:
                        rectTransform.anchoredPosition = new Vector2(-92 * 3, 32);
                        break;
                    case 4:
                        rectTransform.anchoredPosition = new Vector2(-92 * 4, 32);
                        break;
                    case 5:
                        rectTransform.anchoredPosition = new Vector2(-92 * 5, 32);
                        break;
                    case 6:
                        rectTransform.anchoredPosition = new Vector2(-92 * 6, 32);
                        break;
                    case 7:
                        rectTransform.anchoredPosition = new Vector2(-92 * 7, 32);
                        break;
                    case 8:
                        rectTransform.anchoredPosition = new Vector2(-92 * 8, 32);
                        break;
                    case 9:
                        rectTransform.anchoredPosition = new Vector2(-92 * 9, 32);
                        break;
                    case 10:
                        rectTransform.anchoredPosition = new Vector2(-92 * 10, 32);
                        break;
                    case -1:
                        rectTransform.anchoredPosition = new Vector2(92, 32);
                        break;
                    case -2:
                        rectTransform.anchoredPosition = new Vector2(92 * 2, 32);
                        break;
                    case -3:
                        rectTransform.anchoredPosition = new Vector2(92 * 3, 32);
                        break;
                    case -4:
                        rectTransform.anchoredPosition = new Vector2(92 * 4, 32);
                        break;
                    case -5:
                        rectTransform.anchoredPosition = new Vector2(92 * 5, 32);
                        break;
                    case -6:
                        rectTransform.anchoredPosition = new Vector2(92 * 6, 32);
                        break;
                    case -7:
                        rectTransform.anchoredPosition = new Vector2(92 * 7, 32);
                        break;
                    case -8:
                        rectTransform.anchoredPosition = new Vector2(92 * 8, 32);
                        break;
                    case -9:
                        rectTransform.anchoredPosition = new Vector2(92 * 9, 32);
                        break;
                    case -10:
                        rectTransform.anchoredPosition = new Vector2(92 * 10, 32);
                        break;
                    default:
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                }
            }

            else
            {
                switch (-memory)
                {
                    case 0:
                        rectTransform.anchoredPosition = new Vector2(0, 32);
                        break;
                    case 1:
                        rectTransform.anchoredPosition = new Vector2(-92, 32);
                        break;
                    case 2:
                        rectTransform.anchoredPosition = new Vector2(-92 * 2, 32);
                        break;
                    case 3:
                        rectTransform.anchoredPosition = new Vector2(-92 * 3, 32);
                        break;
                    case 4:
                        rectTransform.anchoredPosition = new Vector2(-92 * 4, 32);
                        break;
                    case 5:
                        rectTransform.anchoredPosition = new Vector2(-92 * 5, 32);
                        break;
                    case 6:
                        rectTransform.anchoredPosition = new Vector2(-92 * 6, 32);
                        break;
                    case 7:
                        rectTransform.anchoredPosition = new Vector2(-92 * 7, 32);
                        break;
                    case 8:
                        rectTransform.anchoredPosition = new Vector2(-92 * 8, 32);
                        break;
                    case 9:
                        rectTransform.anchoredPosition = new Vector2(-92 * 9, 32);
                        break;
                    case 10:
                        rectTransform.anchoredPosition = new Vector2(-92 * 10, 32);
                        break;
                    case -1:
                        rectTransform.anchoredPosition = new Vector2(92, 32);
                        break;
                    case -2:
                        rectTransform.anchoredPosition = new Vector2(92 * 2, 32);
                        break;
                    case -3:
                        rectTransform.anchoredPosition = new Vector2(92 * 3, 32);
                        break;
                    case -4:
                        rectTransform.anchoredPosition = new Vector2(92 * 4, 32);
                        break;
                    case -5:
                        rectTransform.anchoredPosition = new Vector2(92 * 5, 32);
                        break;
                    case -6:
                        rectTransform.anchoredPosition = new Vector2(92 * 6, 32);
                        break;
                    case -7:
                        rectTransform.anchoredPosition = new Vector2(92 * 7, 32);
                        break;
                    case -8:
                        rectTransform.anchoredPosition = new Vector2(92 * 8, 32);
                        break;
                    case -9:
                        rectTransform.anchoredPosition = new Vector2(92 * 9, 32);
                        break;
                    case -10:
                        rectTransform.anchoredPosition = new Vector2(92 * 10, 32);
                        break;
                    default:
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                }
            }
            //Debug.Log(Player.localPlayer.mana);
        }
    }

    [Command (requiresAuthority = false)]
    public void CmdChangeMemory(int memory)
    {
        Inst.memory = memory;
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeMemorySameSync(int memory)
    {
        //호스트와 참가자 클라이언트 간의 싱크 오차를 줄이기 위해 RPC를 이용하기 위한 함수
        //RPC에서 실행시켜서 같은 타이밍에 발동되게함
        //원래 memory가 SyncVar라서 Cmd에서만 처리해도 됬지만
        //이러면 호스트는 빠르게 바뀐값이 입력되고 참가자는 느려서 바뀌기 전 값이 입력되서 오차 업애기 위해 작성
        RpcChangeMemorySameSync(memory);
    }

    [ClientRpc]
    public void RpcChangeMemorySameSync(int memory)
    {
        //호스트와 참가자 클라이언트 간의 싱크 오차를 줄이기 위해 RPC를 이용하기 위한 함수
        Inst.memory = memory;

        if (Player.localPlayer.isServer)
        {
            if (MemoryChecker.Inst.memory < 0)
            {
                Player.gameManager.CmdEndTurn();
            }
        }
        else
        {
            if (MemoryChecker.Inst.memory > 0)
            {
                Player.gameManager.CmdEndTurn();
            }
        }
    }

    public void OnMemoryPlusClick()
    {
        if(Player.localPlayer.isServer)
        {
            CmdChangeMemory(Inst.memory + 1);

            if (MemoryChecker.Inst.memory >=10 )
            {
                CmdChangeMemory(10);
            }
        }
        else
        {
            CmdChangeMemory(Inst.memory - 1);

            if (MemoryChecker.Inst.memory <= -10)
            {
                CmdChangeMemory(-10);
            }
        }
    }
    public void OnMemoryMinusClick()
    {
        if (Player.localPlayer.isServer)
        {
            CmdChangeMemory(Inst.memory - 1);
            if (MemoryChecker.Inst.memory < 0)
            {
                Player.gameManager.CmdEndTurn();
            }
        }
        else
        {
            CmdChangeMemory(Inst.memory + 1);
            Debug.Log(Inst.memory);
            if (MemoryChecker.Inst.memory > 0)
            {
                Player.gameManager.CmdEndTurn();
            }
        }
    }

    private void Update()
    {
        //Debug.Log(memory);
    }
    [Command(requiresAuthority =false)]
    public void CmdChangeInstantMemory(int value)
    {
        instantMemory += value;
    }
}
