using UnityEngine;

namespace SecondLaw
{
    public static class LetterCacheService
    {
        public const string CachePrefix = "SecondLaw.Letter.";

        public static string GetOrCreateLetter(LetterTemplate template)
        {
            string key = CachePrefix + template.templateId;
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            string generated = template.body +
                "\n\n-- cached local letter draft --\n" +
                "This text is stored locally now; the same hook can later call an AI provider and cache the result.";
            PlayerPrefs.SetString(key, generated);
            PlayerPrefs.Save();
            return generated;
        }
    }
}
