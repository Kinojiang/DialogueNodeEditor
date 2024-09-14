using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNodeTree.Core
{
    /// <summary>
    /// 分组信息
    /// </summary>
    [Serializable]
    public class CustomGraphGroup
    {
        public string title;
        public Vector2 position;

        public string group_guid;

        public List<string> node_guids = new List<string>();
    }
}
