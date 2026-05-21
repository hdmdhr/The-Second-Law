using UnityEngine;

namespace SecondLaw
{
    public sealed class SlimeAI : MonoBehaviour
    {
        private BattleController battle;
        private Combatant player;
        private Combatant slime;
        private float attackCooldown;

        public void Initialize(BattleController battleController, Combatant playerCombatant)
        {
            battle = battleController;
            player = playerCombatant;
            slime = GetComponent<Combatant>();
        }

        private void Update()
        {
            if (slime == null || player == null || !slime.IsAlive || !player.IsAlive)
            {
                return;
            }

            attackCooldown -= Time.deltaTime;
            if (slime.IsDisabled)
            {
                return;
            }

            Vector2 toPlayer = player.transform.position - slime.transform.position;
            if (Mathf.Abs(toPlayer.x) > 0.15f)
            {
                slime.Facing = toPlayer.x > 0f ? 1 : -1;
            }

            if (Mathf.Abs(toPlayer.x) > 0.8f || Mathf.Abs(toPlayer.y) > 0.35f)
            {
                Vector2 movement = new Vector2(Mathf.Sign(toPlayer.x) * 0.8f, Mathf.Sign(toPlayer.y) * 0.55f);
                if (toPlayer.sqrMagnitude > 1f)
                {
                    movement.Normalize();
                }

                slime.Move(movement * (slime.Stats.moveSpeed * Time.deltaTime));
                slime.PlayLoop(CombatAnimationType.Walk);
                return;
            }

            slime.PlayLoop(CombatAnimationType.Idle);

            if (attackCooldown <= 0f)
            {
                slime.PlayOnce(CombatAnimationType.Attack01, 0.45f);
                battle.PerformMeleeAttack(slime, new[] { player }, 0.95f, 0.42f, slime.Stats.attackPower, 0.9f, 0.25f, 1);
                attackCooldown = 1.35f;
            }
        }
    }
}
