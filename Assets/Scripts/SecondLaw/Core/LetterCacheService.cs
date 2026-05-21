using UnityEngine;

namespace SecondLaw
{
    public static class LetterCacheService
    {
        public const string CachePrefix = "SecondLaw.Letter.";

        public static string GetOrCreateLetter(LetterTemplate template)
        {
            string key = CachePrefix + LocalizationService.CurrentLanguageCode + "." + template.templateId;
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            string generated = LocalizationService.T("letter.slime.body") + LocalizationService.T("cache.note");
            PlayerPrefs.SetString(key, generated);
            PlayerPrefs.Save();
            return generated;
        }
    }
}
