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
        }

        public static string convertToAnim(string sentence)
        {
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
