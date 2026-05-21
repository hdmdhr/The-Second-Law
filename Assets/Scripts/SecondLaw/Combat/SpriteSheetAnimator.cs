using System.Collections.Generic;
using UnityEngine;

namespace SecondLaw
{
    public sealed class SpriteSheetAnimator : MonoBehaviour
    {
        private const int FrameSize = 100;
        private const float PixelsPerUnit = 40f;
        private const float DefaultFrameSeconds = 0.09f;

        private readonly Dictionary<CombatAnimationType, Sprite[]> animations = new Dictionary<CombatAnimationType, Sprite[]>();
        private SpriteRenderer targetRenderer;
        private CombatAnimationType currentAnimation = CombatAnimationType.Idle;
        private CombatAnimationType fallbackLoop = CombatAnimationType.Idle;
        private int frameIndex;
        private float frameTimer;
        private bool playingOnce;
        private float actionLockTimer;

        public bool HasAnimation(CombatAnimationType animationType)
        {
            return animations.TryGetValue(animationType, out Sprite[] frames) && frames.Length > 0;
        }

        public void Initialize(SpriteRenderer renderer, string resourceRoot)
        {
            targetRenderer = renderer;
            animations[CombatAnimationType.Idle] = LoadFrames(resourceRoot + "-Idle");
            animations[CombatAnimationType.Walk] = LoadFrames(resourceRoot + "-Walk");
            animations[CombatAnimationType.Attack01] = LoadFrames(resourceRoot + "-Attack01");
            animations[CombatAnimationType.Attack02] = LoadFrames(resourceRoot + "-Attack02");
            animations[CombatAnimationType.Attack03] = LoadFrames(resourceRoot + "-Attack03");
            animations[CombatAnimationType.Hurt] = LoadFrames(resourceRoot + "-Hurt");
            animations[CombatAnimationType.Death] = LoadFrames(resourceRoot + "-Death");

            PlayLoop(CombatAnimationType.Idle, true);
        }

        private void Update()
        {
            if (targetRenderer == null || !HasAnimation(currentAnimation))
            {
                return;
            }

            if (actionLockTimer > 0f)
            {
                actionLockTimer -= Time.deltaTime;
            }

            Sprite[] frames = animations[currentAnimation];
            frameTimer += Time.deltaTime;
            if (frameTimer < DefaultFrameSeconds)
            {
                return;
            }

            frameTimer = 0f;
            frameIndex++;
            if (frameIndex >= frames.Length)
            {
                if (playingOnce)
                {
                    playingOnce = false;
                    actionLockTimer = 0f;
                    PlayLoop(fallbackLoop, true);
                    return;
                }

                frameIndex = 0;
            }

            targetRenderer.sprite = frames[frameIndex];
        }

        public void PlayLoop(CombatAnimationType animationType, bool force = false)
        {
            if (!force && actionLockTimer > 0f)
            {
                return;
            }

            if (!HasAnimation(animationType))
            {
                return;
            }

            if (!force && currentAnimation == animationType && !playingOnce)
            {
                return;
            }

            currentAnimation = animationType;
            fallbackLoop = animationType;
            playingOnce = false;
            frameIndex = 0;
            frameTimer = 0f;
            targetRenderer.sprite = animations[currentAnimation][frameIndex];
        }

        public void PlayOnce(CombatAnimationType animationType, CombatAnimationType fallback, float lockSeconds = 0.28f)
        {
            if (!HasAnimation(animationType))
            {
                return;
            }

            currentAnimation = animationType;
            fallbackLoop = HasAnimation(fallback) ? fallback : CombatAnimationType.Idle;
            playingOnce = true;
            actionLockTimer = lockSeconds;
            frameIndex = 0;
            frameTimer = 0f;
            targetRenderer.sprite = animations[currentAnimation][frameIndex];
        }

        private static Sprite[] LoadFrames(string resourcePath)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return new Sprite[0];
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int frameCount = Mathf.Max(1, texture.width / FrameSize);
            Sprite[] frames = new Sprite[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                Rect rect = new Rect(i * FrameSize, 0, FrameSize, Mathf.Min(FrameSize, texture.height));
                frames[i] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.18f), PixelsPerUnit);
            }

            return frames;
        }
    }
}
