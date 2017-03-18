using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace EditorTool.PsdExport
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
        private static Dictionary<string , List<Word>> parseMap = new Dictionary<string, List<Word>>();
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
                    import = LayerWordBinder.GetParser(key);
                    
                    if (import == null)
                    {
                        Debug.LogWarning("Cant parse context ! key :" + key + " , layer:" + layerName);
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
                    import = LayerWordBinder.GetParser(key);
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
            //切分字符
            List<Word> words = new List<Word>();
            while (index < charArr.Length)
            {
                buf.Append(charArr[index]);
                if (checkEnd(charArr, index + 1) && buf.Length > 0)
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
                string pArgs = "null";
                if (paramInfo.Length > 1)
                    pArgs = param.Substring(paramInfo[0].Length + 1);
                word.TypeAndParams[paramInfo[0]] = pArgs;
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
            string[] paramsArr = subStr.Split(':');
            word.TypeAndParams = new Dictionary<string, string>();
            string pArgs = "null";
            if (paramsArr.Length > 1)
                pArgs = subStr.Substring(paramsArr[0].Length + 1);

            word.TypeAndParams[paramsArr[0]] = pArgs;
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
                string[] nameArr = newName.Split(':');
                if (nameArr.Length > 1)
                    newName = nameArr[1];                              
            }
            return newName;
        }

    }



}
