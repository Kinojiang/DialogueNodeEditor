using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace CustomNodeTree.Core
{
    public static class HelpTool
    {

        /// <summary>
        /// 利用携程使文字缓慢出现
        /// </summary>
        /// <param name="text"></param>
        /// <param name="data">要显示的文字</param>
        /// <param name="duration">多长时间内显示完毕</param>
        /// <returns></returns>
        public static IEnumerator AppearString(this Text text, string data, float duration)
        {
            //根据字符串长度计算每个字出现的间隔时间
            var time = new WaitForSeconds(data.Length > 0 ? (duration / data.Length) : duration / 24);
            text.text = string.Empty;
            for (int i = 0; i < data.Length; i++)
            {
                text.text += data[i];
                yield return time;
            }
        }

        public static string[] SplitString(string value,string[] split){
            return value.Split(split,System.StringSplitOptions.None);
        }
    }
}

