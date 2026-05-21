using UnityEngine;

namespace SecondLaw
{
    [CreateAssetMenu(menuName = "Second Law/Letter Template")]
    public sealed class LetterTemplate : ScriptableObject
    {
        public string templateId = "slime_thanks";
        public string senderName = "Mina";
        [TextArea] public string body;
        public string[] replyOpenings;
        public string[] replyBodies;
        public string[] replyClosings;
        public int affectionReward = 1;
    }
}
