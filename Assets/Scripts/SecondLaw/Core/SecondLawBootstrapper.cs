using UnityEngine;

namespace SecondLaw
{
    public static class SecondLawBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<SecondLawGame>() != null)
            {
                return;
            }

            GameObject gameObject = new GameObject("Second Law Runtime");
            Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<SecondLawGame>();
        }
    }
}
