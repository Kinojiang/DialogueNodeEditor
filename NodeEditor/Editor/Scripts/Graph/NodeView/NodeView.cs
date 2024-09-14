using System;
using System.Collections.Generic;
using CustomNodeTree.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public CustomNode node;
    //Port为UnityEditor.Experimental.GraphView中的端口
    public Port input;
    public Port outPut;

    public bool isRootNode;

    public Action<NodeView> OnNodeSelected;

    [HideInInspector] public virtual Vector2 default_size => new Vector2(150, 150);
    [HideInInspector] public virtual bool ShowNodeProperties => true;

    public virtual void DrawNode()
    {
    }

    public virtual Capabilities SetCapabilities(Capabilities capabilities)
    {
        return capabilities;
    }

    // public NodeView(CustomNode node,bool isRootNode):base("Assets/NodeEditor/Editor/UI/NodeView.uxml"){
    //     this.node=node;
    //     this.title=node.name;
    //     this.isRootNode=isRootNode;
    //     //可视化面板中的节点的vid与实例节点的guid绑定
    //     this.viewDataKey=node.guid;
    //     this.userData=node;
    //     style.left=node.position.x;
    //     style.top=node.position.y;
    //     CreateInputPorts();
    //     CreateOutputPorts();
    //     SetNodeClass();
    // }

    public virtual void Init(CustomNode node, bool isRootNode)
    {
        this.node = node;
        this.isRootNode = isRootNode;
        //可视化面板中的节点的vid与实例节点的guid绑定
        this.viewDataKey = node.guid;
        this.userData = node;
        style.left = node.position.x;
        style.top = node.position.y;
        //CreateInputPorts();
        //CreateOutputPorts();
        //SetNodeClass();
    }

    private void SetNodeClass()
    {
        if (node is SingleNode)
        {
            AddToClassList("singleNode");
        }
        else if (node is CompositeNode)
        {
            AddToClassList("compositeNode");
        }
    }

    /// <summary>
    /// 创建输入端口
    /// </summary>
    private void CreateInputPorts()
    {
        if (isRootNode)
        {
            Label rootLabel = new Label("RootNode");
            rootLabel.AddToClassList("RootNode");
            inputContainer.Add(rootLabel);
            return;
        }
        /*将节点入口设置为 
            接口链接方向 横向Orientation.Vertical  竖向Orientation.Horizontal
            传输类型 传入Direction.Input  输出Direction.Output
            接口可链接数量 Port.Capacity.Single
            接口类型 typeof(bool)
        */
        // 默认所有节点为多入口类型
        input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
        if (input != null)
        {
            input.portName = "";
            input.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(input);
        }
    }

    /// <summary>
    /// 创建输出端口
    /// </summary>
    private void CreateOutputPorts()
    {
        //如果是单一节点，则只有一个子节点，所以输出端只能有一个连接线
        outPut = InstantiatePort(Orientation.Vertical, Direction.Output, (node is SingleNode) ? Port.Capacity.Single : Port.Capacity.Multi, typeof(bool));
        if (outPut != null)
        {
            outPut.portName = "";
            outPut.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(outPut);
        }
    }

    /// <summary>
    /// 选中该节点时调用
    /// </summary>
    public override void OnSelected()
    {
        base.OnSelected();
        if (OnNodeSelected != null)
        {
            OnNodeSelected.Invoke(this);
        }
    }

    public override void SetPosition(Rect newPos)
    {
        Undo.RecordObject(node, "Node View(SetPosition)");
        base.SetPosition(newPos);
        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;
        EditorUtility.SetDirty(node);
    }

    //根据节点运行状态为对应NodeView添加在uss中的类元素来改变对应样式
    public void SetNodeState()
    {
        RemoveFromClassList("running");
        if (Application.isPlaying)
        {
            switch (node.state)
            {
                case CustomNode.State.Running:
                    if (node.started)
                    {
                        AddToClassList("running");
                    }
                    break;
            }
        }
    }
}

/*
    在序列化时用来存储NodeView中的信息
*/
[Serializable]
public class SerializableNodeView
{
    public List<CustomNode> nodes = new List<CustomNode>();
}
