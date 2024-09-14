using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace CustomNodeTree.Core
{
    [Serializable]
    [NodePortAggregate()]
    [PortCapacity()]
    //用来判断该类使用什么CustomPort
    [DefaultPortType(typeof(CustomPort))]
    public abstract class CustomNode : ScriptableObject
    {
        public enum State
        {
            Running,
            Waiting
        }

        public State state = State.Waiting;
        public bool started = false;
        [HideInInspector] public NodeTree tree;
        [ReadOnly]public CustomNode current;
        //public List<Node> children=new List<Node>();

        [HideInInspector] public string guid;
        /// <summary>
        /// 节点持有的Port
        /// </summary>
        [HideInInspector][SerializeReference] public List<CustomPort> Ports = new List<CustomPort>();

        public CustomNode OnUpdate()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }
            CustomNode currentNode = LogicUpdate();
            if (state != State.Running)
            {
                OnStop();
                started = false;
            }
            return currentNode;
        }

        public abstract CustomNode LogicUpdate();
        public abstract void OnStart();
        public abstract void OnStop();

        /// <summary>
        /// 存储端口信息到节点上
        /// </summary>
        /// <param name="name"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public virtual CustomPort AddPort(string name, CustomPort.PortDirection direction)
        {
            DefaultPortTypeAttribute portAttribute = GetType().GetCustomAttribute<DefaultPortTypeAttribute>();
            Type portType = typeof(CustomPort);
            if (portAttribute != null)
            {
                portType = portAttribute.type;
            }

            CustomPort graphPort = Activator.CreateInstance(portType) as CustomPort;
            graphPort.Name = name;
            graphPort.guid = Guid.NewGuid().ToString();
            graphPort.Direction = direction;
            Ports.Add(graphPort);

            return graphPort;
        }

        public CustomPort FindPortByGuid(string guid)
		{
            return Ports.Where(p => p.guid.Equals(guid) == true).FirstOrDefault();
		}

        #region UNITY_EDITOR

        /// <summary>
        /// If set the node will highlight in the editor for visual feedback at runtime. It is up to the user to disable
        /// other nodes that are active otherwise you will get undesired results in the view.
        /// </summary>
        [HideInInspector] public bool editor_ActiveNode;

#if UNITY_EDITOR
        #region Graph View Editor Values
        [HideInInspector] public Vector2 position;
        [HideInInspector][NonSerialized] public object graphElement;
        #endregion

        public virtual System.Type InputType => typeof(bool);
        public virtual System.Type OutputType => typeof(bool);
#endif
        #endregion
    }

}
