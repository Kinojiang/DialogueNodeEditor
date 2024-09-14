using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomNodeTree;
using CustomNodeTree.Core;
using CustomNodeTreeEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class NodeTreeViewer : GraphView, IEdgeConnectorListener
{
    public new class UxmlFactory : UxmlFactory<NodeTreeViewer, GraphView.UxmlTraits> { }
    public Action<NodeView> OnNodeSelected;
    public NodeTree tree;
    public MiniMap Minimap { get; private set; }
    private GraphSearchWindow searchWindow;
    public Vector2 mousePosInGraph;

    //Runtime 类型 与对应的 Editor 类型
    private Dictionary<Type, Type> visualGraphNodeLookup = new Dictionary<Type, Type>();    //base:CustomNode base:NodeView
    private Dictionary<Type, Type> visualGraphPortLookup = new Dictionary<Type, Type>();    //base:CustomPort base:CustomPortView

    /// <summary>
    /// 图形元素定向，这里用来显示端口的排布
    /// </summary>
    private Orientation orientation;
    public NodeTreeViewer()
    {
        Insert(0, new GridBackground());
        this.AddManipulator(new ContentZoomer());   //视图缩放
        this.AddManipulator(new ContentDragger());  //视图拖拽
        this.AddManipulator(new SelectionDragger());    //选中物体拖拽
        this.AddManipulator(new RectangleSelector());   //框选

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeEditor/Editor/UI/NodeTreeViewer.uss");
        styleSheets.Add(styleSheet);

        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                CustomNodeViewAttribute nodeAttrib = type.GetCustomAttribute<CustomNodeViewAttribute>();
                if (nodeAttrib != null && nodeAttrib.type.IsAbstract == false)
                {
                    //将runtime类型与对应的Editor类型存入字典
                    visualGraphNodeLookup.Add(nodeAttrib.type, type);
                }

                CustomPortViewAttribute portAttrib = type.GetCustomAttribute<CustomPortViewAttribute>();
                if (portAttrib != null && portAttrib.type.IsAbstract == false)
                {
                    visualGraphPortLookup.Add(portAttrib.type, type);
                }
            }
        }

        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        serializeGraphElements += OnSerializeGraphElements;
        unserializeAndPaste += OnUnserializeAndPaste;
        deleteSelection += OnDeleteSelection;

        Undo.undoRedoPerformed += OnUndoRedo;
    }


    private void OnMouseMove(MouseMoveEvent evt)
    {
        // 获取鼠标位置，相对于GraphView的本地坐标系
        mousePosInGraph = this.ChangeCoordinatesTo(this.contentViewContainer, evt.localMousePosition);
    }

    public void CreateMinimap(float windowWidth)
    {
        Minimap = new MiniMap { anchored = true };
        Minimap.capabilities &= ~Capabilities.Movable;
        Minimap.SetPosition(new Rect(windowWidth - 210, 30, 200, 140));
        Add(Minimap);
    }

    #region View GUI/Update
    public void OnGUI()
    {
        if (Minimap != null) Minimap.SetPosition(new Rect(contentRect.width - 210, 30, 200, 140));
    }

    /// <summary>
    /// 先清除视图中的元素,再填充视图
    /// </summary>
    /// <param name="tree"></param>
    internal void PopulateView(NodeTree tree)
    {
        this.tree = tree;
        //这里先取消绑定视图修改的委托，防止触发修改视图连带的修改结点树
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (nodeCreationRequest == null)
        {
            searchWindow = ScriptableObject.CreateInstance<GraphSearchWindow>();
            searchWindow.Configure(this);
            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }

        CreateStartingNode();

        tree.nodes.ForEach(n => CreateNodeView(n));
        tree.nodes.ForEach(n =>
        {
            // if (n is SingleNode && (n as SingleNode).child != null)
            // {
            //     if ((n as SingleNode).child != tree.rootNode)//根节点不能作为子节点
            //     {
            //         NodeView parentView = FindNodeView(n);
            //         NodeView childView = FindNodeView((n as SingleNode).child);
            //         Edge edge = parentView.outPut.ConnectTo(childView.input);
            //         this.AddElement(edge);
            //     }
            //     else
            //     {
            //         NodeView parentView = FindNodeView(n);
            //         NodeView childView = FindNodeView((n as SingleNode).child);
            //         tree.RemoveChild(parentView.node, childView.node);
            //     }

            // }
            // else if (n is CompositeNode)
            // {
            //     var children = tree.GetChldren(n);
            //     for (int i = 0; i < children.Count; i++)
            //     {
            //         if (children[i] != tree.rootNode)
            //         {
            //             NodeView parentView = FindNodeView(n);
            //             NodeView childView = FindNodeView(children[i]);
            //             Edge edge = parentView.outPut.ConnectTo(childView.input);
            //             this.AddElement(edge);
            //         }
            //         else
            //         {
            //             NodeView parentView = FindNodeView(n);
            //             NodeView childView = FindNodeView(children[i]);
            //             tree.RemoveChild(parentView.node, childView.node);
            //         }
            //     }
            // }

            //遍历CustomNode中的Ports
            n.Ports.ForEach(graphPort =>
            {
                if (graphPort.Direction == CustomPort.PortDirection.Output)
                {
                    Port port = graphPort.editor_port as Port;
                    //CustomPort的Connections保存了一条连接另一个端口得信息的线
                    graphPort.Connections.ForEach(graphConnectin =>
                    {
                        //Debug.Log(graphConnectin.Node);
                        CustomPort inputGraphPort = tree.FindNodeByGuid(graphConnectin.node_guid).FindPortByGuid(graphConnectin.port_guid);
                        Port inputPort = inputGraphPort.editor_port as Port;
                        AddElement(port.ConnectTo(inputPort));
                    });
                }
            });
        });

        tree.Groups.ForEach(group => DisplayGroup(group));
    }

    #endregion

    #region 菜单栏相关
    /// <summary>
    /// 创建视图中的菜单选项
    /// </summary>
    /// <param name="evt"></param>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);
        //TypeCache.GetTypesDerivedFrom<T>(),通过此方法可以快速访问 Unity 域程序集中派生自特定类或实现特定接口的所有类。基类或接口可为通用类型。
        // var types = TypeCache.GetTypesDerivedFrom<CustomNodeTree.Core.Node>();
        // foreach (var type in types)
        // {
        //     if (type.IsAbstract) continue;
        //     //添加菜单中可执行行为
        //     evt.menu.AppendAction(GetCategoryAttributeName(type), a => CreateNode(type), selection.Count > 0 ? DropdownMenuAction.Status.Hidden : DropdownMenuAction.Status.Normal);
        // }
        //selection不会为空，只能用Count做判断是否选中元素
        //启用菜单：DropdownMenuAction.Status.Normal 禁用菜单：DropdownMenuAction.Status.Disabled
        //evt.menu.AppendAction($"Delete", a => DeleteSelection(), selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
        //evt.menu.AppendAction($"SetRootTreeNode", a => SetRootTreeNode(), selection.Count > 0 && selection.Count < 2 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
        //分割线
        //evt.menu.AppendSeparator();
        evt.menu.AppendAction("Group Selection", a =>
            {
                CreateNewGroup(selection.OfType<GraphElement>());
            }, selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
        evt.menu.AppendAction("UGroup Selection"
        , a =>
        {
            RemoveElementsFromGroup(selection.OfType<GraphElement>());
        }, selection.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden);
    }

    public string GetCategoryAttributeName(Type type)
    {
        NodeCategoryAttribute nodeCategoryAttribute = type.GetCustomAttribute<NodeCategoryAttribute>();
        if (nodeCategoryAttribute != null) return nodeCategoryAttribute.Category;
        return null;
    }

    /// <summary>
    /// 复制选中的元素
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    private string OnSerializeGraphElements(IEnumerable<GraphElement> elements)
    {
        //用来存储要序列化的信息，JsonUtility.ToJson支持序列化单个实例，所有要新实例化一个类来集中存储要序列化的信息
        //JsonUtility.ToJson直接序列化List是不行的
        SerializableNodeView serializeTarget = new SerializableNodeView();
        elements.ToList().ForEach(e =>
        {
            NodeView nodeView = e as NodeView;
            if (nodeView != null)
            {
                serializeTarget.nodes.Add(nodeView.node);
            }
        }
        );
        string text = JsonUtility.ToJson(serializeTarget);
        return text;
    }

    /// <summary>
    /// 利用反序列化实现粘贴
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="data"></param>
    private void OnUnserializeAndPaste(string operationName, string data)
    {
        SerializableNodeView jsonData = JsonUtility.FromJson<SerializableNodeView>(data);
        Debug.Log("Paset");
        foreach (var node in jsonData.nodes)
        {
            CreateNode(node.GetType());
        }
    }

    /// <summary>
    /// 将选定节点设置为根节点，并重新填充视图
    /// </summary>
    public void SetRootTreeNode()
    {
        tree.rootNode = (selection[0] as NodeView).node;
        PopulateView(tree);
        EditorUtility.SetDirty(tree);
    }

    private void OnUndoRedo()
    {
        PopulateView(tree);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 创建新的视图分组
    /// </summary>
    /// <param name="elements"></param>
    public void CreateNewGroup(IEnumerable<GraphElement> elements)
    {
        GraphGroupView newGroup = new GraphGroupView();
        newGroup.InitGroup(tree.AddGroup(typeof(CustomGraphGroup)));
        newGroup.AddElements(elements);
        AddElement(newGroup);
    }

    List<GraphElement> addElements = new List<GraphElement>();
    /// <summary>
    /// 显示已经有的视图分组
    /// </summary>
    /// <param name="group"></param>
    public void DisplayGroup(CustomGraphGroup group)
    {
        GraphGroupView newGroup = new GraphGroupView();
        newGroup.InitGroup(group);
        newGroup.title = group.title;
        addElements.Clear();
        group.node_guids.ForEach(ng => addElements.Add(GetNodeByGuid(ng)));
        newGroup.AddElements(addElements);
        AddElement(newGroup);
    }

    /// <summary>
    /// 从视图中移除元素
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="groupView"></param>
    public virtual void RemoveElementsFromGroup(IEnumerable<GraphElement> elements, GraphGroupView groupView = null)
    {
        foreach (var element in elements)
        {
            foreach (var group in tree.Groups)
            {
                if (group.node_guids.Contains(element.viewDataKey))
                {
                    (GetElementByGuid(group.group_guid) as GraphGroupView).ManualRemoveElement(element);
                }
            }
        }
        EditorUtility.SetDirty(tree);
    }


    #endregion

    #region  GraphChange

    /// <summary>
    /// 视图修改所绑定的委托，graphViewChanged用来绑定这个
    /// </summary>
    /// <param name="graphViewChange"></param>
    /// <returns></returns>
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        //视图中元素被移除时
        if (graphViewChange.elementsToRemove != null)
        {
            //连接线删除时存储在elementsToRemove中的位置要比其相关的点的位置靠前
            //就拿下列代码为例，删除一个节点视图时会先删除其相关的连接线，再来删除节点视图
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                NodeView nodeView = elem as NodeView;
                if (nodeView != null)
                {
                    tree.DeleteNode(nodeView.node);
                }
                //判断并删除连接线相关
                Edge edge = elem as Edge;
                if (edge != null)
                {
                    CustomNode graph_input_node = edge.input.node.userData as CustomNode;
                    CustomNode graph_output_node = edge.output.node.userData as CustomNode;
                    Undo.RecordObjects(new UnityEngine.Object[] { graph_input_node, graph_output_node }, "Add Port to Node");
                    RemoveEdge(edge);
                }

                if (typeof(Group).IsInstanceOfType(elem) == true)
                {
                    Debug.Log("Remove Group");
                    tree.RemoveGroup((elem as GraphGroupView).Group);
                }
            });
        }

        //视图中创建连接线时
        // if (graphViewChange.edgesToCreate != null)
        // {
        //     graphViewChange.edgesToCreate.ForEach(edge =>
        //     {
        //         NodeView parentView = edge.output.node as NodeView;
        //         NodeView childView = edge.input.node as NodeView;
        //         tree.AddChild(parentView.node, childView.node);
        //     });
        // }

        EditorUtility.SetDirty(tree);
        return graphViewChange;
    }

    #endregion

    #region  Delete
    private List<Group> removeGroups = new List<Group>();
    private void OnDeleteSelection(string operationName, AskUser askUser)
    {
        removeGroups.Clear();
        var currentDeleteElement = selection.OfType<GraphElement>();
        foreach (var element in currentDeleteElement)
        {
            if (typeof(Group).IsInstanceOfType(element) == true)
            {
                removeGroups.Add(element as Group);
            }
        }

        //要删除的选中项中包含Group时，只删除选中项
        if (removeGroups.Count > 0)
        {
            HashSet<GraphElement> hashSet = new HashSet<GraphElement>();
            foreach (var element in selection.OfType<GraphElement>())
            {
                if (!hashSet.Contains(element))
                {
                    hashSet.Add(element);
                }
            }
            HashSet<GraphElement> hashSet2 = new HashSet<GraphElement>();
            foreach (Placemat item in from p in hashSet.OfType<Placemat>()
                                      where p.Collapsed
                                      select p)
            {
                hashSet2.UnionWith(item.CollapsedElements);
                item.Collapsed = false;
            }

            DeleteElements(hashSet);
            selection.Clear();
            foreach (GraphElement item2 in hashSet2)
            {
                AddToSelection(item2);
            }
        }
        else
        {
            //该方法删除选中项的同时会把其子项一并删除
            DeleteSelection();
        }
    }
    #endregion
    #region  CreateNode

    /// <summary>
    /// 通过Guid寻找视图中相应的视图节点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private NodeView FindNodeView(CustomNode node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }


    public void CreateStartingNode()
    {
        if (tree.rootNode == null)
        {
            StartingNode startingNode = ScriptableObject.CreateInstance<StartingNode>();
            startingNode.name = "Start";
            startingNode.tree=tree;
            startingNode.current = startingNode;
            startingNode.guid = System.Guid.NewGuid().ToString();
            startingNode.position = new Vector2(270, 30);

            CustomPort graphPort = startingNode.AddPort("Next", CustomPort.PortDirection.Output);
            graphPort.CanBeRemoved = false;

            tree.rootNode = startingNode;
            tree.nodes.Add(startingNode);
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tree))) AssetDatabase.AddObjectToAsset(startingNode, tree);

            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// 更新视图节点的状态
    /// </summary>
    public void UpdateNodeStates()
    {
        //nodes是这张视图中保存的视图节点
        nodes.ForEach(n =>
        {
            NodeView view = n as NodeView;
            view.SetNodeState();
        });
    }

    /// <summary>
    ///在结点树中创建节点并在视图中创建节点
    /// </summary>
    /// <param name="type"></param>
    public void CreateNode(Type type)
    {
        CustomNode node = tree.CreateNode(type);
        node.position = mousePosInGraph;

        NodePortAggregateAttribute dynamicsAttrib = node.GetType().GetCustomAttribute<NodePortAggregateAttribute>();
        Debug.Assert(dynamicsAttrib != null, $"Graph node requires a NodePortAggregateAttribute {node.GetType().Name}");

        // PortCapacityAttribute capacityAttrib = node.GetType().GetCustomAttribute<PortCapacityAttribute>();
        // Debug.Assert(capacityAttrib != null, $"Graph node requires a PortCapacityAttribute {node.GetType().Name}");

        if (dynamicsAttrib.InputPortAggregate != NodePortAggregateAttribute.PortAggregate.None)
        {
            CustomPort graphPort = node.AddPort("Input", CustomPort.PortDirection.Input);
            graphPort.CanBeRemoved = false;
        }

        if (dynamicsAttrib.OutputPortAggregate == NodePortAggregateAttribute.PortAggregate.Single)
        {
            CustomPort graphPort = node.AddPort("Exit", CustomPort.PortDirection.Output);
            graphPort.CanBeRemoved = false;
        }
        CreateNodeView(node);
    }

    /// <summary>
    /// 根据节点在视图中创建相应的NodeView
    /// </summary>
    /// <param name="node"></param>
    private NodeView CreateNodeView(CustomNode node)
    {

        Type nodeViewType = typeof(NodeView);
        if (visualGraphNodeLookup.ContainsKey(node.GetType()) == true)
        {
            nodeViewType = visualGraphNodeLookup[node.GetType()];
        }

        NodeView nodeView = Activator.CreateInstance(nodeViewType) as NodeView;
        nodeView.Init(node, node == tree.rootNode);
        nodeView.title = GetGraphNodeName(node.GetType());
        nodeView.AddToClassList("NodeView");
        nodeView.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeEditor/Editor/UI/NodeView.uss"));

        //通过获取节点类上的CustomNodeStyleAttribute判断当前视图节点是否需要添加新的样式
        IEnumerable<CustomNodeStyleAttribute> customStyleAttribs = node.GetType().GetCustomAttributes<CustomNodeStyleAttribute>();
        if (customStyleAttribs != null)
        {
            foreach (var customStyleAttrib in customStyleAttribs)
            {
                try
                {
                    StyleSheet styleSheet = Resources.Load<StyleSheet>(customStyleAttrib.style);
                    if (styleSheet != null)
                    {
                        nodeView.styleSheets.Add(styleSheet);
                    }
                    else throw new Exception();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{ex.Data}: Style sheet does not exit: {customStyleAttrib.style}");
                }
            }
        }

        //通过获取节点类上的NodePortAggregateAttribute判断当前视图节点是否需能够手动添加输入输出端口
        NodePortAggregateAttribute dynamicsAttrib = node.GetType().GetCustomAttribute<NodePortAggregateAttribute>();
        Debug.Assert(dynamicsAttrib != null, $"Graph node requires a NodePortAggregateAttribute {node.GetType().Name}");

        //可手动添加复数端口时，我们在视图中添加一个增加端口的按钮
        if (dynamicsAttrib.InputPortAggregate == NodePortAggregateAttribute.PortAggregate.Multiple)
        {
            // 输入可增加端口时
            var button = new Button(() => { CreatePort(nodeView, "Input", CustomPort.PortDirection.Input); })
            {
                text = "Add Input"
            };
            nodeView.titleButtonContainer.Add(button);
        }
        if (dynamicsAttrib.OutputPortAggregate == NodePortAggregateAttribute.PortAggregate.Multiple)
        {
            var button = new Button(() => { CreatePort(nodeView, "Exit", CustomPort.PortDirection.Output); })
            {
                text = "Add Exit"
            };
            nodeView.titleButtonContainer.Add(button);
        }

        //将CustomNode中Ports存在的CustomPort以CustomPortView的形式添加在视图节点上
        foreach (var graphPort in node.Ports)
        {
            AddPort(graphPort, nodeView);
        }

        nodeView.DrawNode();

        // 在视图节点上展示CustomNode中的资产
        if (nodeView.ShowNodeProperties)
        {
            VisualElement divider = new VisualElement();
            divider.style.borderBottomColor = divider.style.borderTopColor = divider.style.borderLeftColor = divider.style.borderRightColor = Color.black;
            divider.style.borderBottomWidth = divider.style.borderTopWidth = divider.style.borderLeftWidth = divider.style.borderRightWidth = 0.5f;
            nodeView.mainContainer.Add(divider);

            VisualElement node_data = new VisualElement();
            node_data.AddToClassList("node_data");

            Foldout mainFoldout=new Foldout();
            mainFoldout.name="mainFoldout";
            mainFoldout.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeEditor/Editor/UI/FoldoutStyle.uss"));
            mainFoldout.Add(node_data);
            nodeView.mainContainer.Add(mainFoldout);

            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor((CustomNode)nodeView.userData);
            IMGUIContainer inspectorIMGUI = new IMGUIContainer(() => { editor.OnInspectorGUI(); });
            node_data.Add(inspectorIMGUI);
        }

        // // 最后将该节点视图添加到当前视图上
        this.AddElement(nodeView);

        // 设置节点功能。可以覆盖默认的“视图”节点
        nodeView.capabilities = nodeView.SetCapabilities(nodeView.capabilities);

        //nodeView.SetPosition(new Rect(node.position, nodeView.default_size));
        //NodeView nodeView = new NodeView(node, node == tree.rootNode);
        //将NodeView中的OnNodeSelected与该脚本自身的OnNodeSelected相关联
        nodeView.OnNodeSelected = OnNodeSelected;
        return nodeView;
        // 将节点元素添加到该视图中
    }

    /// <summary>
    /// 指定类型上标记了NodeNameAttribute即可获得其中的名字
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private string GetGraphNodeName(Type type)
    {
        string display_name = "";
        if (type.GetCustomAttribute<NodeNameAttribute>() != null)
        {
            display_name = type.GetCustomAttribute<NodeNameAttribute>().name;
        }
        else
        {
            display_name = type.Name;
        }
        return display_name;
    }



    /// <summary>
    /// 移除连接线
    /// </summary>
    /// <param name="edge"></param>
    private void RemoveEdge(Edge edge)
    {
        CustomPort graph_input_port = edge.input.userData as CustomPort;
        CustomPort graph_output_port = edge.output.userData as CustomPort;
        graph_input_port.RemoveConnectionByPortGuid(graph_output_port.guid);
        graph_output_port.RemoveConnectionByPortGuid(graph_input_port.guid);
    }
    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {

    }

    /// <summary>
    /// 当连接线连接上新的端口时调用
    /// </summary>
    /// <param name="graphView"></param>
    /// <param name="edge"></param>
    public void OnDrop(GraphView graphView, Edge edge)
    {
        CustomNode graph_input_node = edge.input.node.userData as CustomNode;
        CustomNode graph_output_node = edge.output.node.userData as CustomNode;

        Undo.RecordObjects(new UnityEngine.Object[] { graph_input_node, graph_output_node }, "Add Port to Node");

        CustomPort graph_input_port = edge.input.userData as CustomPort;
        CustomPort graph_output_port = edge.output.userData as CustomPort;

        graph_input_port.Connections.Add(new CustomPort.CustomPortConnection()
        {
            initialized = true,
            Node = edge.output.node.userData as CustomNode,
            port = graph_output_port,
            port_guid = graph_output_port.guid,
            node_guid = graph_output_node.guid
        });
        graph_output_port.Connections.Add(new CustomPort.CustomPortConnection()
        {
            initialized = true,
            Node = edge.input.node.userData as CustomNode,
            port = graph_input_port,
            port_guid = graph_input_port.guid,
            node_guid = graph_input_node.guid
        });

        EditorUtility.SetDirty(tree);
    }
    #endregion

    #region Create Port
    /// <summary>
    /// 根据方向为给定节点创建端口。为图中的节点创建端口后
    /// 将向视图节点添加一个端口
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <param name="direction"></param>
    public void CreatePort(Node node, string name, CustomPort.PortDirection direction)
    {
        CustomNode graphNode = node.userData as CustomNode;
        Undo.RecordObject(graphNode, "Add Port to Node");

        CustomPort graphPort = graphNode.AddPort(name, direction);
        AddPort(graphPort, node);

        EditorUtility.SetDirty(tree);
    }


    /// <summary>
    ///在视图结点上添加一个端口
    /// </summary>
    /// <param name="graphPort"></param>
    /// <param name="node"></param>
    private void AddPort(CustomPort graphPort, Node node)
    {
        CustomNode graphNode = node.userData as CustomNode;

        // 判断要添加的端口是输入端口还是输出端口
        Direction direction = (graphPort.Direction == CustomPort.PortDirection.Input) ? Direction.Input : Direction.Output;

        // 获取节点上的PortCapacityAttribute来判断该端口是支持单个或者多个连接
        PortCapacityAttribute capacityAttrib = graphNode.GetType().GetCustomAttribute<PortCapacityAttribute>();
        Debug.Assert(capacityAttrib != null, $"Graph node requires a PortCapacityAttribute {graphNode.GetType().Name}");
        //视图节点中的端口的可连接数量
        Port.Capacity capacity = Port.Capacity.Single;
        //先判断是输入还是输出节点
        if (graphPort.Direction == CustomPort.PortDirection.Input)
        {
            capacity = (capacityAttrib.InputPortCapacity == PortCapacityAttribute.Capacity.Single) ? Port.Capacity.Single : Port.Capacity.Multi;
        }
        else
        {
            capacity = (capacityAttrib.OutputPortCapacity == PortCapacityAttribute.Capacity.Single) ? Port.Capacity.Single : Port.Capacity.Multi;
        }

        // 获取视图端口类型，这里默认是bool类型，在节点类中可定义
        // TODO: can we optimze/change this to be more dynamic?
        Type port_type = (graphPort.Direction == CustomPort.PortDirection.Input) ? graphNode.InputType : graphNode.OutputType;

        // 创建视图节点上的端口
        var port = node.InstantiatePort(orientation, direction, capacity, port_type);
        port.portName = "";// Don't set the name this helps with the view.
        port.userData = graphPort;
        graphPort.editor_port = port;

        // 自定义视图节点上的端口
        NodePortAggregateAttribute portAggregateAttrib = graphNode.GetType().GetCustomAttribute<NodePortAggregateAttribute>();
        NodePortAggregateAttribute.PortAggregate aggregate = NodePortAggregateAttribute.PortAggregate.None;
        if (graphPort.Direction == CustomPort.PortDirection.Input)
        {
            if (portAggregateAttrib.InputPortAggregate != NodePortAggregateAttribute.PortAggregate.None)
                aggregate = (portAggregateAttrib.InputPortAggregate == NodePortAggregateAttribute.PortAggregate.Single) ? NodePortAggregateAttribute.PortAggregate.Single : NodePortAggregateAttribute.PortAggregate.Multiple;
        }
        else
        {
            if (portAggregateAttrib.OutputPortAggregate != NodePortAggregateAttribute.PortAggregate.None)
                aggregate = (portAggregateAttrib.OutputPortAggregate == NodePortAggregateAttribute.PortAggregate.Single) ? NodePortAggregateAttribute.PortAggregate.Single : NodePortAggregateAttribute.PortAggregate.Multiple;
        }



        CustomportView graphPortView = null;
        if (aggregate != NodePortAggregateAttribute.PortAggregate.None)
        {
            Type portViewType = null;
            //根据自定义端口信息来获取自定义视图端口
            visualGraphPortLookup.TryGetValue(graphPort.GetType(), out portViewType);
            if (portViewType == null)
            {
                portViewType = typeof(CustomGraphLabelPortView);
            }
            Debug.Log(portViewType.Name);
            graphPortView = Activator.CreateInstance(portViewType) as CustomportView;
        }

        if (graphPortView != null)
        {
            graphPortView.CreateView(graphPort);
            port.Add(graphPortView);
        }

        // 如果可移除端口
        if (graphPort.CanBeRemoved)
        {
            var deleteButton = new Button(() => RemovePort(node, port))
            {
                text = "X"
            };
            port.Add(deleteButton);
        }
        //添加与VisualElement关联的操纵器。
        port.AddManipulator(new EdgeConnector<Edge>(this));

        // 将端口添加到视图节点
        if (graphPortView != null)
        {
            if (direction == Direction.Input)
            {
                node.inputContainer.Add(port);
            }
            else
            {
                node.outputContainer.Add(port);
            }
        }
        //刷新视图节点
        node.RefreshExpandedState();
        node.RefreshPorts();
    }


    /// <summary>
    /// 从节点视图上移除端口
    /// </summary>
    /// <param name="node"></param>
    /// <param name="socket"></param>
    private void RemovePort(Node node, Port socket)
    {
        CustomPort socket_port = socket.userData as CustomPort;
        List<Edge> edgeList = edges.ToList();
        //先移除与端口相关的线
        foreach (var edge in edgeList)
        {
            if (socket_port.Direction == CustomPort.PortDirection.Output)
            {
                CustomPort graphPort = edge.output.userData as CustomPort;
                if (graphPort.guid.Equals(socket_port.guid))
                {
                    RemoveEdge(edge);
                    edge.input.Disconnect(edge);
                    RemoveElement(edge);
                }
            }
            else
            {
                CustomPort graphPort = edge.input.userData as CustomPort;
                if (graphPort.guid.Equals(socket_port.guid))
                {
                    RemoveEdge(edge);
                    edge.output.Disconnect(edge);
                    RemoveElement(edge);
                }
            }

        }

        CustomNode graphNode = node.userData as CustomNode;

        Undo.RecordObject(graphNode, "Remove Port");

        //再CustomNode的Ports里移除该CutomPort
        graphNode.Ports.Remove(socket_port);

        //最后从视图节点里移除对应端口
        if (socket.direction == Direction.Input)
        {
            node.inputContainer.Remove(socket);
        }
        else
        {
            node.outputContainer.Remove(socket);
        }
        node.RefreshPorts();
        node.RefreshExpandedState();

        EditorUtility.SetDirty(tree);
    }

    /// <summary>
    /// GraphView中的虚方法作用如下：
    /// 获取与给定端口兼容的所有端口，
    /// 其实就是判断所选节点的端口能与其它哪些端口相连接
    /// 在这里用来限制所选开始端口能连接到那些其它端口
    /// </summary>
    /// <param name="startPort"></param>
    /// <param name="nodeAdapter"></param>
    /// <returns></returns>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        //Where为Linq中的扩展方法，用来从迭代器中获取符合条件的物体
        //返回与startPort的direction不同且与startPort所属的Node也不同的Port
        return ports.ToList().Where(endport => endport.direction != startPort.direction
        && endport.node != startPort.node).ToList();
    }
    #endregion
}
