using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;

namespace Skype3D.CharacterUtil
{
    public class Words2Anim
    {
        public static string vocabularyPath = "ms-appx:///Assets/skype3d_vocabulary.txt";
        public static Dictionary<string, List<string>> words2Anim = new Dictionary<string, List<string>>();
        private static Random rnd = new Random();
        public static Dictionary<string, string> specialDic = new Dictionary<string, string>();

        public static async Task LoadVocabulary()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(vocabularyPath));

            using (var inputStream = await file.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                while (streamReader.Peek() > 0)
                {
                    string line = streamReader.ReadLine();
                    string[] words = line.Split();
                    for (int i = 1; i < words.Length; i++)
                    {
                        if (words2Anim.ContainsKey(words[i]))
                            words2Anim[words[i]].Add(words[0]);
                        else
                        {
                            List<string> anims = new List<string>();
                            anims.Add(words[0]);
                            words2Anim.Add(words[i], anims);
                        }
                    }
                }
            }

            specialDic.Add("go for exercise", "run_00");
            specialDic.Add("passed my graduation", "jump_11");
            specialDic.Add("my muscle", "pose_01");
            specialDic.Add("Chinese Kungfu", "special_21");
            specialDic.Add("feel so bad", "sit_03");

            specialDic.Add("welcome new intern", "greet_02");
            specialDic.Add("welcome buddy", "handshake_00");
            specialDic.Add("warm welcome", "offerhug_00");
            specialDic.Add("nice to meet you", "greet_00");
        }

        public static string special(string sentence)
        {
            foreach (KeyValuePair<string, string> entry in specialDic)
            {
                if (sentence.Contains(entry.Key))
                    return entry.Value;
            }
            return null;
        }

        public static string convertToAnim(string sentence)
        {
            string specialResult = special(sentence);
            if (specialResult != null)
                return specialResult;
            List<string> animCandidates = new List<string>();
            foreach (KeyValuePair<string, List<string>> entry in words2Anim)
            {
                if (sentence.Contains(entry.Key))
                    animCandidates.AddRange(entry.Value);
            }
            if (animCandidates.Count > 0)
                return animCandidates[rnd.Next(animCandidates.Count)];
            else
                return null;
        }
    }
}
