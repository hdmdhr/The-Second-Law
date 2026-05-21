using System;
using UnityEngine;

namespace SecondLaw
{
    public sealed class Combatant : MonoBehaviour
    {
        public CharacterStats Stats { get; private set; }
        public float CurrentHealth { get; private set; }
        public float CurrentStamina { get; private set; }
        public int CurrentAmmo { get; private set; }
        public int Facing { get; set; } = 1;
        public bool IsAlive { get; private set; } = true;
        public bool IsPlayer { get; private set; }
        public Transform VisualRoot { get; private set; }
        public SpriteRenderer BodyRenderer { get; private set; }
        public SpriteSheetAnimator Animator { get; private set; }
        public float Depth => transform.position.y;
        public bool IsDisabled => stunTimer > 0f || charmTimer > 0f;
        public bool IsGuarding { get; set; }

        public event Action<Combatant> Died;

        private float stunTimer;
        private float charmTimer;
        private float reloadTimer;
        private Vector2 knockbackVelocity;
        private Color normalColor = Color.white;

        public void Initialize(CharacterStats stats, Sprite sprite, bool isPlayer)
        {
            Stats = stats;
            CurrentHealth = stats.maxHealth;
            CurrentStamina = stats.maxStamina;
            CurrentAmmo = stats.maxAmmo;
            IsPlayer = isPlayer;

            VisualRoot = new GameObject("Visual").transform;
            VisualRoot.SetParent(transform, false);
            BodyRenderer = VisualRoot.gameObject.AddComponent<SpriteRenderer>();
            BodyRenderer.sprite = sprite;
            normalColor = isPlayer ? new Color(0.9f, 0.92f, 1f) : new Color(0.6f, 1f, 0.55f);
            BodyRenderer.color = normalColor;
        }

        public void AttachSpriteSheetAnimator(string resourceRoot)
        {
            if (BodyRenderer == null)
            {
                return;
            }

            Animator = VisualRoot.gameObject.AddComponent<SpriteSheetAnimator>();
            Animator.Initialize(BodyRenderer, resourceRoot);
            if (Animator.HasAnimation(CombatAnimationType.Idle))
            {
                BodyRenderer.color = Color.white;
                normalColor = Color.white;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (!IsAlive)
            {
                return;
            }

            if (stunTimer > 0f)
            {
                stunTimer -= dt;
            }

            if (charmTimer > 0f)
            {
                charmTimer -= dt;
                if (BodyRenderer != null)
                {
                    BodyRenderer.color = Color.Lerp(normalColor, new Color(1f, 0.42f, 0.86f), 0.55f);
                }
            }
            else
            {
                if (BodyRenderer != null)
                {
                    BodyRenderer.color = normalColor;
                }
            }

            if (Stats.maxStamina > 0)
            {
                CurrentStamina = Mathf.Min(Stats.maxStamina, CurrentStamina + Stats.staminaRegenPerSecond * dt);
            }

            if (Stats.maxAmmo > 0 && CurrentAmmo < Stats.maxAmmo)
            {
                reloadTimer += dt;
                if (reloadTimer >= Stats.reloadSeconds)
                {
                    CurrentAmmo = Stats.maxAmmo;
                    reloadTimer = 0f;
                }
            }

            if (knockbackVelocity.sqrMagnitude > 0.01f)
            {
                transform.position += (Vector3)(knockbackVelocity * dt);
                knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, dt * 6f);
            }

            if (BodyRenderer != null)
            {
                BodyRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
                VisualRoot.localScale = new Vector3(Facing, 1f, 1f);
            }
        }

        public bool Spend(int stamina, int ammo)
        {
            if (stamina > CurrentStamina || ammo > CurrentAmmo)
            {
                return false;
            }

            CurrentStamina -= stamina;
            CurrentAmmo -= ammo;
            if (ammo > 0 && CurrentAmmo <= 0)
            {
                reloadTimer = 0f;
            }

            return true;
        }

        public void ReloadNow()
        {
            if (Stats.maxAmmo <= 0)
            {
                return;
            }

            CurrentAmmo = Stats.maxAmmo;
            reloadTimer = 0f;
        }

        public void TakeDamage(float rawDamage, Vector2 knockback, float stunSeconds, bool breaksCharm = true)
        {
            if (!IsAlive)
            {
                return;
            }

            if (breaksCharm)
            {
                charmTimer = 0f;
            }

            float guardMultiplier = IsGuarding && CurrentStamina >= 8f ? 0.35f : 1f;
            if (IsGuarding && CurrentStamina >= 8f)
            {
                CurrentStamina -= 8f;
            }

            float damage = Mathf.Max(1f, rawDamage * guardMultiplier - Stats.defense);
            CurrentHealth -= damage;
            stunTimer = Mathf.Max(stunTimer, stunSeconds);
            knockbackVelocity += knockback;
            PlayOnce(CombatAnimationType.Hurt);
            if (Animator == null)
            {
                StartCoroutine(Flash());
            }

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public void Charm(float seconds)
        {
            charmTimer = Mathf.Max(charmTimer, seconds);
            stunTimer = Mathf.Max(stunTimer, seconds);
        }

        public void Move(Vector2 delta)
        {
            if (!IsAlive || IsDisabled)
            {
                return;
            }

            transform.position += (Vector3)delta;
        }

        private System.Collections.IEnumerator Flash()
        {
            if (BodyRenderer == null)
            {
                yield break;
            }

            BodyRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            BodyRenderer.color = normalColor;
        }

        private void Die()
        {
            IsAlive = false;
            PlayOnce(CombatAnimationType.Death, 0.8f);
            if (Animator == null)
            {
                BodyRenderer.color = new Color(0.55f, 0.55f, 0.55f, 0.85f);
            }
            Died?.Invoke(this);
            Destroy(gameObject, 0.8f);
        }

        public void PlayLoop(CombatAnimationType animationType)
        {
            if (Animator != null)
            {
                Animator.PlayLoop(animationType);
            }
        }

        public void PlayOnce(CombatAnimationType animationType, float lockSeconds = 0.28f)
        {
            if (Animator != null)
            {
                Animator.PlayOnce(animationType, CombatAnimationType.Idle, lockSeconds);
            }
        }
    }
}
