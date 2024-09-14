using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using CustomNodeTree.Core;

namespace CustomNodeTreeEditor
{
    public class NodeEditor : EditorWindow
    {
        public NodeTreeViewer nodeTreeViewer;

        [MenuItem("Window/UI Toolkit/NodeEditor")]
        public static void ShowExample()
        {
            NodeEditor wnd = GetWindow<NodeEditor>();
            wnd.titleContent = new GUIContent("NodeEditor");
        }

        //双击资源时触发
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is NodeTree)
            {
                ShowExample();
                return true;
            }
            return false;
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/NodeEditor/Editor/UI/NodeEditor.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/NodeEditor/Editor/UI/NodeEditor.uss");
            root.styleSheets.Add(styleSheet);

            nodeTreeViewer = root.Q<NodeTreeViewer>();
            nodeTreeViewer.CreateMinimap(this.position.width);


            //一般情况下该函数会在创建完EditorWindow界面后再由触发条件调用，这里手动调用一下，以便一开始就进行视图填充
            OnSelectionChange();
        }



        /// <summary>
        /// 每当选择发生更改时调用,跟MonoBehavior中的Start之类的相似
        /// </summary>
        private void OnSelectionChange()
        {
            NodeTree tree = Selection.activeObject as NodeTree;
            if (tree == null) return;
            nodeTreeViewer.PopulateView(tree);
        }

        /// <summary>
        /// 以每秒 10 帧的速度调用，以便检视面板有机会进行更新,跟MonoBehavior中的Start之类的相似
        /// </summary>
        private void OnInspectorUpdate()
        {
            nodeTreeViewer?.UpdateNodeStates();
        }

        /// <summary>
        /// 当GUI更新时会调用该方法
        /// </summary>
        private void OnGUI()
        {
            nodeTreeViewer?.OnGUI();
        }
    }
}
