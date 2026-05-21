using UnityEngine;

namespace SecondLaw
{
    public sealed class Projectile : MonoBehaviour
    {
        private BattleController battle;
        private Combatant owner;
        private float damage;
        private float speed;
        private float maxDistance;
        private float depthTolerance;
        private float traveled;
        private int direction;

        public void Initialize(BattleController battleController, Combatant source, Sprite sprite, float damageValue, float projectileSpeed, float range, float depth, int facing)
        {
            battle = battleController;
            owner = source;
            damage = damageValue;
            speed = projectileSpeed;
            maxDistance = range;
            depthTolerance = depth;
            direction = facing;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = owner.BodyRenderer.sortingOrder + 1;
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position += Vector3.right * direction * step;
            traveled += step;

            Combatant target = battle.FindFirstEnemyInLine(owner, transform.position, 0.35f, depthTolerance);
            if (target != null)
            {
                target.TakeDamage(damage, new Vector2(direction * 1.4f, 0f), 0.12f);
                Destroy(gameObject);
                return;
            }

            if (traveled >= maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
