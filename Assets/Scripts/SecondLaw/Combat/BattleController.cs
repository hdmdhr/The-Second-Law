using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public sealed class BattleController : MonoBehaviour
    {
        public Combatant Player { get; private set; }
        public int KillCount { get; private set; }

        private SecondLawGame game;
        private QuestDefinition quest;
        private readonly List<Combatant> enemies = new List<Combatant>();
        private Sprite bulletSprite;
        private bool ending;

        public static BattleController Create(SecondLawGame game, QuestDefinition quest)
        {
            GameObject root = new GameObject("Battle Controller");
            BattleController controller = root.AddComponent<BattleController>();
            controller.game = game;
            controller.quest = quest;
            controller.BuildBattle();

            HudController hud = Object.FindFirstObjectByType<HudController>();
            if (hud != null)
            {
                hud.Bind(controller);
            }

            return controller;
        }

        public IReadOnlyList<Combatant> Enemies => enemies;

        private void BuildBattle()
        {
            CreateArena();
            bulletSprite = SpriteFactory.MakeRectSprite("Bullet", new Color(1f, 0.88f, 0.25f), 24, 8);

            Sprite playerSprite = SpriteFactory.MakeSprite("UniformValkyrie", new Color(0.30f, 0.48f, 0.96f), new Color(1f, 0.87f, 0.95f));
            Player = CreateCombatant("Uniform Valkyrie", game.Progression.BuildPlayerStats(game.PlayerBaseStats), playerSprite, new Vector2(-3.2f, -0.6f), true);
            Player.gameObject.AddComponent<PlayerController>().Initialize(this, game.Progression);
            Player.Died += _ => EndBattle(false);

            Sprite slimeSprite = SpriteFactory.MakeSprite("Slime", new Color(0.35f, 0.86f, 0.38f), new Color(0.78f, 1f, 0.65f));
            for (int i = 0; i < quest.targetCount; i++)
            {
                Vector2 position = new Vector2(1.8f + i * 1.35f, -0.95f + i * 0.55f);
                Combatant slime = CreateCombatant("Slime " + (i + 1), game.SlimeBaseStats.CloneRuntime(), slimeSprite, position, false);
                slime.gameObject.AddComponent<SlimeAI>().Initialize(this, Player);
                slime.Died += OnEnemyDied;
                enemies.Add(slime);
            }
        }

        private Combatant CreateCombatant(string name, CharacterStats stats, Sprite sprite, Vector2 position, bool player)
        {
            GameObject entity = new GameObject(name);
            entity.transform.SetParent(transform, false);
            entity.transform.position = position;
            Combatant combatant = entity.AddComponent<Combatant>();
            combatant.Initialize(stats, sprite, player);
            return combatant;
        }

        private void CreateArena()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, -0.1f, -10f);
                camera.orthographicSize = 5.3f;
                camera.backgroundColor = new Color(0.06f, 0.08f, 0.10f);
            }

            GameObject floor = new GameObject("Guild Road Arena");
            floor.transform.SetParent(transform, false);
            floor.transform.position = new Vector3(0f, -1.1f, 1f);
            SpriteRenderer floorRenderer = floor.AddComponent<SpriteRenderer>();
            floorRenderer.sprite = SpriteFactory.MakeRectSprite("ArenaFloor", new Color(0.23f, 0.20f, 0.16f), 256, 96);
            floorRenderer.drawMode = SpriteDrawMode.Sliced;
            floorRenderer.size = new Vector2(12f, 4.6f);
            floorRenderer.sortingOrder = -1000;

            GameObject back = new GameObject("Forest Backdrop");
            back.transform.SetParent(transform, false);
            back.transform.position = new Vector3(0f, 2.2f, 1f);
            SpriteRenderer backRenderer = back.AddComponent<SpriteRenderer>();
            backRenderer.sprite = SpriteFactory.MakeRectSprite("Backdrop", new Color(0.12f, 0.21f, 0.18f), 256, 96);
            backRenderer.drawMode = SpriteDrawMode.Sliced;
            backRenderer.size = new Vector2(12f, 3.2f);
            backRenderer.sortingOrder = -1001;
        }

        public void PerformMeleeAttack(Combatant attacker, IEnumerable<Combatant> targets, float range, float depthTolerance, float damage, float knockback, float stun, int maxTargets = 99)
        {
            int hitCount = 0;
            foreach (Combatant target in targets)
            {
                if (target == null || !target.IsAlive || target == attacker)
                {
                    continue;
                }

                float dx = target.transform.position.x - attacker.transform.position.x;
                float dy = Mathf.Abs(target.Depth - attacker.Depth);
                bool inFront = Mathf.Sign(dx) == attacker.Facing || Mathf.Abs(dx) < 0.2f;
                if (inFront && Mathf.Abs(dx) <= range && dy <= depthTolerance)
                {
                    target.TakeDamage(damage, new Vector2(attacker.Facing * knockback, 0f), stun);
                    hitCount++;
                    if (hitCount >= maxTargets)
                    {
                        return;
                    }
                }
            }
        }

        public void PerformCharm(Combatant attacker, float range, float depthTolerance, float seconds)
        {
            foreach (Combatant enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                float dx = enemy.transform.position.x - attacker.transform.position.x;
                float dy = Mathf.Abs(enemy.Depth - attacker.Depth);
                if (Mathf.Sign(dx) == attacker.Facing && Mathf.Abs(dx) <= range && dy <= depthTolerance)
                {
                    enemy.Charm(seconds);
                }
            }
        }

        public void FireProjectile(Combatant owner, float damage, float range, float depthTolerance, float speed = 9f)
        {
            GameObject projectileObject = new GameObject("Bullet");
            projectileObject.transform.SetParent(transform, false);
            projectileObject.transform.position = owner.transform.position + new Vector3(owner.Facing * 0.55f, 0.35f, 0f);
            projectileObject.AddComponent<Projectile>().Initialize(this, owner, bulletSprite, damage, speed, range, depthTolerance, owner.Facing);
        }

        public Combatant FindFirstEnemyInLine(Combatant owner, Vector3 position, float xRadius, float depthTolerance)
        {
            IReadOnlyList<Combatant> targets = owner.IsPlayer ? (IReadOnlyList<Combatant>)enemies : new[] { Player };
            foreach (Combatant target in targets)
            {
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                if (Mathf.Abs(target.transform.position.x - position.x) <= xRadius && Mathf.Abs(target.Depth - owner.Depth) <= depthTolerance)
                {
                    return target;
                }
            }

            return null;
        }

        private void OnEnemyDied(Combatant enemy)
        {
            KillCount++;
            enemies.Remove(enemy);
            if (KillCount >= quest.targetCount)
            {
                EndBattle(true);
            }
        }

        private void EndBattle(bool victory)
        {
            if (ending)
            {
                return;
            }

            ending = true;
            StartCoroutine(EndAfterDelay(victory));
        }

        private IEnumerator EndAfterDelay(bool victory)
        {
            yield return new WaitForSeconds(0.8f);
            game.FinishBattle(victory);
        }
    }
}
