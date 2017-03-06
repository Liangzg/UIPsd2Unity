using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace subjectnerdagreement.psdexport
{

    public class Word
    {

        public Dictionary<string , string > TypeAndParams = new Dictionary<string, string>();
        public string Context;

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.AppendLine(Context);
            if (TypeAndParams != null)
            {
                foreach (string key in TypeAndParams.Keys)
                {
                    buf.AppendFormat("{0}:{1}\n", key, TypeAndParams[key]);
                }
            }
            return buf.ToString();
        }
    }

    /// <summary>
    /// 字符解析
    /// </summary>
    public class WordParser
    {
        public static Dictionary<string , List<Word>> parseMap = new Dictionary<string, List<Word>>();
        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="context"></param>
        public static List<Word> BindingParse(GameObject context, string layerName)
        {
            List<Word> words = ParseLayerName(context.name);
            string newName = getGameObjectName(words);
            if(!string.IsNullOrEmpty(newName))
                context.name = newName;

            IBinding import = null;
            //解析绑定组件和参数
            foreach (Word word in words)
            {
                if(word.TypeAndParams == null)  continue;
                foreach (string key in word.TypeAndParams.Keys)
                {
                    if (key != "component")
                        import = LayerWordBinder.GetParser(key);
                    else
                        import = LayerWordBinder.GetParser(word.TypeAndParams[key]);

                    if (import == null)
                    {
                        Debug.LogWarning("Cant parse context ! key :" + key + " , value:" + word.TypeAndParams[key]);
                        continue;
                    }

                    import.StartBinding(context , word.TypeAndParams[key] , layerName);
                }
            }

            return words;
        }


        public static void exitBindingParse(List<Word> words , GameObject context , string layerName)
        {
            IBinding import = null;
            foreach (Word word in words)
            {
                if (word.TypeAndParams == null) continue;
                foreach (string key in word.TypeAndParams.Keys)
                {
                    if (key != "component")
                        import = LayerWordBinder.GetParser(key);
                    else
                        import = LayerWordBinder.GetParser(word.TypeAndParams[key]);

                    if (import == null) continue;

                    import.ExitBinding(context, word.TypeAndParams[key], layerName);
                }
            }
        }


        public static List<Word> ParseLayerName(string layerName)
        {
            if (parseMap.ContainsKey(layerName)) return parseMap[layerName];

            char[] charArr = layerName.ToCharArray();
            int index = 0;
            StringBuilder buf = new StringBuilder();
            List<Word> words = new List<Word>();
            while (index < charArr.Length)
            {
                buf.Append(charArr[index]);
                if (checkEnd(charArr, index + 1) && buf.Length > 1)
                {
                    Word newWord = new Word();
                    newWord.Context = buf.ToString().TrimEnd();
                    words.Add(newWord);

                    buf.Length = 0;
                }
                index++;
            }

            //解析组件和参数
            foreach (Word word in words)
            {
                if (word.Context.StartsWith("["))
                {
                    splitSquareBrackets(word);
                }
                else if (word.Context.StartsWith("@"))
                {
                    splitWidgetComponent(word);
                }
            }

            parseMap[layerName] = words;

            return words;
        }


        public static string[] GetTextureExportPath(string layerName)
        {
            List<Word> words = ParseLayerName(layerName);
            PsdSetting setting = PsdSetting.Instance;
            foreach (Word word in words)
            {
                if(!word.TypeAndParams.ContainsKey("img"))   continue;

                string paramStr = word.TypeAndParams["img"];
                string[] imgInfo = paramStr.Split('_'); //asset type

                string assetFolder = setting.GetAssetFolder(imgInfo[0]);
                string assetName = assetFolder == setting.DefaultImportPath ? paramStr : paramStr.Substring(imgInfo[0].Length + 1);
                return new []{assetName,assetFolder};
            }

            return new []
            {
                layerName,
                setting.DefaultImportPath
            };
        }

        private static bool checkEnd(char[] charArr, int nextIndex)
        {
            if (nextIndex >= charArr.Length)
                return true;

            if (charArr[nextIndex] == '[' || charArr[nextIndex] == '@')
                return true;

            return false;
        }

        /// <summary>
        /// 切分[]括号
        /// 参数形式： [anchor:lr|img:common_pic001]
        /// </summary>
        /// <returns>返回字符总共的长度，包括括号本身</returns>
        private static void splitSquareBrackets(Word word)
        {
            string subStr = word.Context.Substring(1, word.Context.Length - 2);
            string[] args = subStr.TrimEnd().Split('|');

            foreach (string param in args)
            {
                string[] paramInfo = param.Split(':');
                if (paramInfo.Length < 2)
                {
                    throw new Exception("Cant identify params ! arg is " + param);
                }
                word.TypeAndParams[paramInfo[0]] = paramInfo[1];
            }
        }

        /// <summary>
        /// 切分组件名称
        /// </summary>
        /// <param name="word"></param>
        private static void splitWidgetComponent(Word word)
        {
            int index = word.Context.IndexOf("_");
            string subStr = word.Context.Substring(1, index > 0 ? index : word.Context.Length - 1);
            string[] componentAndParams = subStr.Split(':');
            word.TypeAndParams = new Dictionary<string, string>();
            word.TypeAndParams["component"] = componentAndParams[0];
        }


        private static string getGameObjectName(List<Word> words)
        {
            if (words.Count == 0)
            {
                return string.Empty;
            }
            string newName = words[0].Context;
            if (newName.IndexOfAny(new[] {'[', ']'}) >= 0)
            {
                newName = newName.Replace("[", "").Replace("]", "");
                newName = newName.Split(':')[1];
            }
            return newName;
        }
    }



}
