using System;
using UnityEditor;
using UnityEngine;

namespace CustomNodeTree
{
    //给类添加的特性，用来存储分类信息的字段
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeCategoryAttribute : Attribute
    {
        private string mCategory;
        public string Category => mCategory;

        public NodeCategoryAttribute(string categoryPath)
        {
            mCategory = categoryPath;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeNameAttribute : Attribute
    {
        public string name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public NodeNameAttribute(string _name)
        {
            name = _name;
        }
    }

    /// <summary>
    /// 结点树默认使用的节点类型，在NodeTree及其子类上使用，传入的Type类型是CustomNode及其子类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DefaultNodeTypeAttribute : Attribute
    {
        public Type type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public DefaultNodeTypeAttribute(Type _type)
        {
            type = _type;
        }
    }

    /// <summary>
    /// 默认端口类型，在CustomNode结点上使用，传入的Type值为CustomPort或其子类或自定义存储端口信息的类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DefaultPortTypeAttribute : Attribute
    {
        public Type type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public DefaultPortTypeAttribute(Type _type)
        {
            type = _type;
        }
    }

    /// <summary>
    /// 在NodeView或其子类上使用，传入的Type为CustomNode及子类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomNodeViewAttribute : Attribute
    {
        public Type type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public CustomNodeViewAttribute(Type type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// 在CustomPortView或其子类上使用，传入的Type为CustomPort及子类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomPortViewAttribute : Attribute
    {
        public Type type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public CustomPortViewAttribute(Type type)
        {
            this.type = type;
        }
    }

    /// <summary>
    /// 在CustomNode或其子类上使用，用来在与CustomNode对应的视图节点上添加指定样式
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomNodeStyleAttribute : Attribute
    {
        public string style;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public CustomNodeStyleAttribute(string style)
        {
            this.style = style;
        }
    }

    /// <summary>
    /// 节点的端口详情，用来限制节点的输入输出方可存在的端口数量，请放在CustomNode类以及其子类上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodePortAggregateAttribute : Attribute
    {
        public enum PortAggregate
        {
            None,
            Single,
            Multiple
        };
        public PortAggregate InputPortAggregate = PortAggregate.Single;
        public PortAggregate OutputPortAggregate = PortAggregate.Multiple;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public NodePortAggregateAttribute(PortAggregate InputPortDynamics = PortAggregate.Single, PortAggregate OutputPortDynamics = PortAggregate.Multiple)
        {
            this.InputPortAggregate = InputPortDynamics;
            this.OutputPortAggregate = OutputPortDynamics;
        }
    }

    /// <summary>
    /// 视图节点端口可连接的数量，在继承自CustomNode的类上使用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PortCapacityAttribute : Attribute
    {
        public enum Capacity
        {
            Single = 0,
            Multi = 1
        }
        public Capacity InputPortCapacity = Capacity.Multi;
        public Capacity OutputPortCapacity = Capacity.Single;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_name"></param>
        public PortCapacityAttribute(Capacity InputPortCapacity = Capacity.Multi, Capacity OutputPortCapacity = Capacity.Single)
        {
            this.InputPortCapacity = InputPortCapacity;
            this.OutputPortCapacity = OutputPortCapacity;
        }
    }

    /// <summary>
    /// 在Inspector界面仅读
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute {}


    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Cache the current GUI enabled state
            bool prevGuiEnabledState = GUI.enabled;

            //GUI.enabled=false后禁用所有 GUI 交互。所有控件都将以半透明方式绘制，并且不会响应用户输入
            //容以理解一点的说法就是将GUI.enabled设为false后，在其重新变为true前，这期间渲染的GUI不可交互
            //比如你需要让某项GUI不能交互，你可以用以下代码方式实现
            //GUI.enabled=false;
            // 渲染代码
            //GUI.enabled=true;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            //这里立马将GUI.enabled设为true，防止影响其它GUI渲染
            GUI.enabled = prevGuiEnabledState;
        }
    }
}


