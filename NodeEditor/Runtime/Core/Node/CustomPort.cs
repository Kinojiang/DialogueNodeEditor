using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomNodeTree.Core
{
    /// <summary>
    /// 节点中的端口信息
    /// </summary>
    [Serializable]
    public class CustomPort
    {
        /// <summary>
        /// Port 方向
        /// </summary>
        public enum PortDirection
        {
            Input,
            Output
        };

        /// <summary>
        /// 端口所持有的连接线
        /// </summary>
        [Serializable]
        public class CustomPortConnection
        {
            //NonSerialized 是标记的物体不可序列化
            [NonSerialized] public bool initialized;
            [NonSerialized] public CustomNode Node;   // 连接的节点，持有该线的节点所连接的另一个节点
            [NonSerialized] public CustomPort port;   // 连接的端口
 
            public string node_guid;                       // Reference to the port that belongs to the Node
            public string port_guid;                       // Reference to the port that belongs to the Node
        }

        // internals
        [HideInInspector] public string Name;
        [HideInInspector] public PortDirection Direction;
        [HideInInspector] public bool CanBeRemoved = true;
        [HideInInspector] public string guid;

        /// <summary>
        /// 保存了连接置另一个端口的连接线
        /// </summary>
        /*[HideInInspector]*/[SerializeField] public List<CustomPortConnection> Connections = new List<CustomPortConnection>();

        /// <summary>
        /// Initialize
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// 通过GUID寻找与端口相关连接线
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public CustomPortConnection FindConnectionByPortGuid(string guid)
        {
            return Connections.Where(c => c.port_guid.Equals(guid) == true).FirstOrDefault();
        }

        /// <summary>
        /// 通过GUID寻找与节点相关连接线
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public CustomPortConnection FindConnectionByNodeGuid(string guid)
        {
            return Connections.Where(c => c.node_guid.Equals(guid) == true).FirstOrDefault();
        }

        /// <summary>
        /// 从CustomPort的Connection中移除符合guid的连接线
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveConnectionByPortGuid(string guid)
        {
            CustomPortConnection connection = Connections.Where(p => p.port_guid.Equals(guid) == true).FirstOrDefault();
            if (connection != null)
            {
                Connections.Remove(connection);
            }
        }

        /// <summary>
        /// 移除CustomPort中的connections 
        /// </summary>
        public void ClearConnections()
        {
            foreach(CustomPortConnection connection in Connections)
            {
                if (connection.port != null)
                {
                    connection.port.RemoveConnectionByPortGuid(guid);
                }
            }
            Connections.Clear();
        }

        #region UNITY_EDITOR
#if UNITY_EDITOR
        // The Editor Port (easier to link)
        public object editor_port;
#endif
		#endregion

	}
}