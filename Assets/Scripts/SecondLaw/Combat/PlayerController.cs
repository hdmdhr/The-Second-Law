using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public sealed class PlayerController : MonoBehaviour
    {
        private BattleController battle;
        private ProgressionService progression;
        private Combatant combatant;
        private readonly Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        private float jumpTimer;
        private const float JumpDuration = 0.46f;
        private const float ComboWindow = 0.85f;
        private float comboTimer;
        private ComboDirection comboDirection = ComboDirection.None;
        private bool shortcutPrimed;
        private bool movedThisFrame;

        private enum ComboDirection
        {
            None,
            Up,
            Down,
            Forward
        }

        private enum ComboAction
        {
            Attack,
            Jump,
            Shoot
        }

        public void Initialize(BattleController battleController, ProgressionService progressionService)
        {
            battle = battleController;
            progression = progressionService;
            combatant = GetComponent<Combatant>();
        }

        private void Update()
        {
            if (combatant == null || !combatant.IsAlive)
            {
                return;
            }

            TickCooldowns();
            TickCombo();
            HandleMovement();
            combatant.PlayLoop(movedThisFrame ? CombatAnimationType.Walk : CombatAnimationType.Idle);
            HandleJumpVisual();
            HandleActions();
        }

        private void HandleMovement()
        {
            Vector2 input = Vector2.zero;
            if (Input.GetKey(KeyCode.A)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D)) input.x += 1f;
            if (Input.GetKey(KeyCode.S)) input.y -= 1f;
            if (Input.GetKey(KeyCode.W)) input.y += 1f;

            if (input.x != 0f)
            {
                combatant.Facing = input.x > 0f ? 1 : -1;
            }

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            movedThisFrame = input.sqrMagnitude > 0.01f;

            Vector2 delta = input * (combatant.Stats.moveSpeed * Time.deltaTime);
            Vector3 next = combatant.transform.position + (Vector3)delta;
            next.x = Mathf.Clamp(next.x, -5.5f, 5.5f);
            next.y = Mathf.Clamp(next.y, -2.55f, 1.25f);
            combatant.Move((Vector2)(next - combatant.transform.position));
        }

        private void HandleJumpVisual()
        {
            if (jumpTimer > 0f)
            {
                jumpTimer -= Time.deltaTime;
                float t = 1f - jumpTimer / JumpDuration;
                float height = Mathf.Sin(t * Mathf.PI) * combatant.Stats.jumpHeight;
                combatant.VisualRoot.localPosition = new Vector3(0f, height, 0f);
            }
            else
            {
                combatant.VisualRoot.localPosition = Vector3.zero;
            }
        }

        private void HandleActions()
        {
            combatant.IsGuarding = Input.GetKey(KeyCode.L);
            if (combatant.IsDisabled)
            {
                return;
            }

            bool upPressed = Input.GetKeyDown(KeyCode.W);
            bool downPressed = Input.GetKeyDown(KeyCode.S);
            bool forwardPressed = combatant.Facing > 0 ? Input.GetKeyDown(KeyCode.D) : Input.GetKeyDown(KeyCode.A);

            if (Input.GetKeyDown(KeyCode.L))
            {
                StartDefendCombo();
                return;
            }

            if (comboTimer > 0f)
            {
                if (upPressed)
                {
                    comboDirection = ComboDirection.Up;
                }
                else if (downPressed)
                {
                    comboDirection = ComboDirection.Down;
                }
                else if (forwardPressed)
                {
                    comboDirection = ComboDirection.Forward;
                }

                if (Input.GetKeyDown(KeyCode.J))
                {
                    if (shortcutPrimed)
                    {
                        UseShortcutSkill();
                    }
                    else
                    {
                        ResolveDefendCombo(ComboAction.Attack);
                    }

                    return;
                }

                if (Input.GetKeyDown(KeyCode.K))
                {
                    if (comboDirection == ComboDirection.None)
                    {
                        shortcutPrimed = true;
                    }
                    else
                    {
                        ResolveDefendCombo(ComboAction.Jump);
                    }

                    return;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ResolveDefendCombo(ComboAction.Shoot);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseSkill(SkillInputCommand.UpAttack);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                UseSkill(SkillInputCommand.Shoot);
                return;
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                BasicAttack();
                return;
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                Jump();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                combatant.ReloadNow();
            }
        }

        private void StartDefendCombo()
        {
            comboTimer = ComboWindow;
            comboDirection = ComboDirection.None;
            shortcutPrimed = false;
        }

        private void ResolveDefendCombo(ComboAction action)
        {
            SkillInputCommand? command = null;

            if (comboDirection == ComboDirection.Up && action == ComboAction.Jump)
            {
                command = SkillInputCommand.UpJump;
            }
            else if (comboDirection == ComboDirection.Forward && action == ComboAction.Shoot)
            {
                command = SkillInputCommand.ForwardShoot;
            }
            else if (comboDirection == ComboDirection.Down && action == ComboAction.Jump)
            {
                command = SkillInputCommand.DownJump;
            }
            else if (comboDirection == ComboDirection.Forward && action == ComboAction.Jump)
            {
                command = SkillInputCommand.ForwardJump;
            }
            else if (comboDirection == ComboDirection.Up && action == ComboAction.Attack)
            {
                command = SkillInputCommand.UpAttack;
            }

            comboTimer = 0f;
            shortcutPrimed = false;
            comboDirection = ComboDirection.None;

            if (command.HasValue)
            {
                UseSkill(command.Value);
            }
            else if (action == ComboAction.Attack)
            {
                BasicAttack();
            }
            else if (action == ComboAction.Jump)
            {
                Jump();
            }
            else
            {
                UseSkill(SkillInputCommand.Shoot);
            }
        }

        private void UseShortcutSkill()
        {
            comboTimer = 0f;
            shortcutPrimed = false;
            comboDirection = ComboDirection.None;
            UseSkill(SkillInputCommand.UpJump);
        }

        private void BasicAttack()
        {
            combatant.PlayOnce(CombatAnimationType.Attack01);
            float damage = combatant.Stats.attackPower;
            battle.PerformMeleeAttack(combatant, battle.Enemies, 1.15f, 0.42f, damage, 1.2f, 0.2f);

            SkillDefinition closeDefense = progression.FindSkill(SkillInputCommand.Passive);
            if (progression.State.level >= 8 && closeDefense != null)
            {
                battle.FireProjectile(combatant, damage * 0.55f, 2.2f, 0.38f, 10f);
            }
        }

        private void Jump()
        {
            if (jumpTimer <= 0f)
            {
                jumpTimer = JumpDuration;
            }
        }

        private void UseSkill(SkillInputCommand command)
        {
            SkillDefinition skill = progression.FindSkill(command);
            if (skill == null || !progression.IsSkillUnlocked(skill) || IsCoolingDown(skill) || !combatant.Spend(skill.staminaCost, skill.ammoCost))
            {
                return;
            }

            cooldowns[skill.skillId] = skill.cooldownSeconds;
            float damage = combatant.Stats.attackPower * skill.damageMultiplier;

            switch (skill.effectType)
            {
                case SkillEffectType.Uppercut:
                    combatant.PlayOnce(CombatAnimationType.Attack01, 0.42f);
                    jumpTimer = JumpDuration;
                    combatant.transform.position += Vector3.right * combatant.Facing * 0.35f;
                    battle.PerformMeleeAttack(combatant, battle.Enemies, skill.range, skill.depthTolerance, damage, skill.knockback, skill.stunSeconds);
                    break;
                case SkillEffectType.Bullet:
                    combatant.PlayOnce(CombatAnimationType.Attack02, 0.28f);
                    battle.FireProjectile(combatant, damage, skill.range, skill.depthTolerance);
                    break;
                case SkillEffectType.BurstShot:
                    combatant.PlayOnce(CombatAnimationType.Attack02, 0.58f);
                    combatant.transform.position -= Vector3.right * combatant.Facing * 0.25f;
                    StartCoroutine(FireBurst(skill, damage));
                    break;
                case SkillEffectType.Knockdown:
                    combatant.PlayOnce(CombatAnimationType.Attack03, 0.5f);
                    battle.PerformMeleeAttack(combatant, battle.Enemies, skill.range, skill.depthTolerance, damage, skill.knockback, skill.stunSeconds, 3);
                    break;
                case SkillEffectType.Charm:
                    combatant.PlayOnce(CombatAnimationType.Attack03, 0.38f);
                    battle.PerformCharm(combatant, skill.range, skill.depthTolerance, skill.stunSeconds);
                    break;
                case SkillEffectType.MachineGun:
                    combatant.PlayOnce(CombatAnimationType.Attack03, 0.72f);
                    StartCoroutine(FireMachineGun(skill, damage));
                    battle.PerformMeleeAttack(combatant, battle.Enemies, skill.range, skill.depthTolerance, damage, skill.knockback, skill.stunSeconds);
                    break;
            }
        }

        private System.Collections.IEnumerator FireBurst(SkillDefinition skill, float damage)
        {
            for (int i = 0; i < 4; i++)
            {
                battle.FireProjectile(combatant, damage, skill.range, skill.depthTolerance, 11f);
                yield return new WaitForSeconds(0.08f);
            }
        }

        private System.Collections.IEnumerator FireMachineGun(SkillDefinition skill, float damage)
        {
            for (int i = 0; i < 8; i++)
            {
                battle.FireProjectile(combatant, damage, skill.range, skill.depthTolerance, 12f);
                yield return new WaitForSeconds(0.06f);
            }
        }

        private bool IsCoolingDown(SkillDefinition skill)
        {
            return cooldowns.ContainsKey(skill.skillId) && cooldowns[skill.skillId] > 0f;
        }

        private void TickCooldowns()
        {
            if (cooldowns.Count == 0)
            {
                return;
            }

            List<string> keys = new List<string>(cooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                cooldowns[keys[i]] -= Time.deltaTime;
            }
        }

        private void TickCombo()
        {
            if (comboTimer <= 0f)
            {
                return;
            }

            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                shortcutPrimed = false;
                comboDirection = ComboDirection.None;
            }
        }
    }
}
