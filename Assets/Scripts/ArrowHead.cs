using UnityEngine;

public class ArrowHead : MonoBehaviour
{
    [Header("Arrow Heads")]
    public Sprite defaultHead;
    public Sprite targetHead;

    [Header("Properties")]
    public SpriteRenderer spriteRenderer;
    public LayerMask targetLayer;

    [HideInInspector] public CardInfo card;

    public void FindTargets(Entity caster, Vector2 mousePos, bool IsAbility)
    {
        RaycastHit2D[] hitInfo = Physics2D.CircleCastAll(mousePos, 0.1f, Vector2.zero, 1f, targetLayer);

        // If greater than 0, then we hit s
        if (hitInfo.Length > 0)
        {
            RaycastHit2D hit;
            Entity target=null;
            for (int i =0; i<hitInfo.Length; ++i)
            {
                if (hitInfo[i].collider.gameObject.GetComponent<Entity>()!=null)
                {
                    hit = hitInfo[i];
                    target = hit.collider.gameObject.GetComponent<Entity>();
                    break;
                }
            }
            //RaycastHit2D hit = hitInfo[0];
            //target = hit.collider.gameObject.GetComponent<Entity>();

            if (target == null) return;

            bool canTarget;

            if (((FieldCard)caster).CanAttack())
            { 
                //통상 크리쳐카드가 공격하는거라면 
                if(target is Player)
                {
                    //플레이어라면 그냥타겟 ㄱㄱ
                    canTarget = target.casterType.CanTarget(card.acceptableTargets);
                }
                else
                {
                    //필드카드라면 레스트 상태인 적만 공격
                    canTarget = (target.casterType.CanTarget(card.acceptableTargets) && target.GetComponent<FieldCard>().attacked);
                }
                //Debug.Log(((FieldCard)caster).isSecurity + ((FieldCard)caster).card.name);
            }
            else if (((FieldCard)caster).card.data is SpellCard && IsAbility && ((FieldCard)caster).isSecurity == false)
            {
                //통상 스펠카드가 마법부여하는거라면
                canTarget = target.casterType.CanTarget(card.acceptableTargets);
            }
            else if ((IsAbility && ((FieldCard)caster).isSecurity))
            {
                //캐스터가 시큐리티 카드인 경우
                canTarget = target.casterType.CanTarget(((SpellCard)card.data).acceptableSecurityTargets);
            }
            else if (((FieldCard)caster).card.data is CreatureCard creatureCard&& IsAbility)
            {
                //크리쳐 카드인데 isAbility가 true로 발동된다면
                //buffTarget리스트 안에 내용물로 타게팅
                canTarget = creatureCard.buffTargets.Contains(target.casterType);
            }
            else
            { 
                canTarget = false; 
            }

            // Check to see if we can attack this target : If entity isn't the one currently targeting, is targetable and isn't friendly
            if (target && !target.isTargeting && target.isTargetable && canTarget)
            {
                spriteRenderer.sprite = targetHead;
                if (Input.GetMouseButtonDown(0))
                {
                    if (!IsAbility)
                    {
                        //일반적인 상대 골라 공격
                        ((CreatureCard)card.data).Attack(caster, target);
                    }
                    //else if(!IsAbility && caster.CantAttack() && ((FieldCard)caster).securityAttack>0 && target is Player)
                    //{
                    //    //공격할 순 없는데 세큐리티 어택이 남았을 때 세큐리티 어택을 했을 경우 
                    //    ((CreatureCard)card.data).Attack(caster, target);
                    //}
                    else 
                    { 
                        ((ScriptableCard)card.data).StartCast(caster, target);
                    }
                }
            }
            else
            {
                spriteRenderer.sprite = defaultHead;
            }
        }
        else if (spriteRenderer.sprite != defaultHead)
        {
            spriteRenderer.sprite = defaultHead;
        }
    }
}
