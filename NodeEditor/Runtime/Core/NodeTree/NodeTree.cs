using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CustomNodeTree.Core
{
    [CreateAssetMenu(fileName = "NodeTree", menuName = "NodeEditor/NodeTree")]
    public class NodeTree : ScriptableObject
    {
        [ReadOnly]public CustomNode rootNode;
        public CustomNode runningNode;
        public CustomNode.State treeState = CustomNode.State.Waiting;
        public List<CustomNode> nodes = new List<CustomNode>();
        //CustomGraphGroup只在编辑器里使用，游戏运行时用不到，所以也标记为不可序列化
        [HideInInspector]public List<CustomGraphGroup> Groups=new List<CustomGraphGroup>();

        public virtual void Update()
        {
            if (treeState == CustomNode.State.Running && runningNode.state == CustomNode.State.Running)
            {
                runningNode = runningNode.OnUpdate();
            }
        }

        /// <summary>
        /// 对话树开始的触发方法
        /// </summary>
        public virtual void OnTreeStart()
        {
            treeState = CustomNode.State.Running;
            runningNode = rootNode;
            runningNode.state = CustomNode.State.Running;
        }

        /// <summary>
        /// 对话树结束的触发方法
        /// </summary>
        public virtual void OnTreeEnd()
        {
            treeState = CustomNode.State.Waiting;
        }

        /// <summary>
        /// 将树的运行状态初始化
        /// </summary>
        public virtual void InitTree()
        {
            nodes.ForEach(n =>
            {
                n.started = false;
                n.state = CustomNode.State.Waiting;
            });
            OnTreeEnd();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 根据节点类型创建节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public CustomNode CreateNode(System.Type type)
        {
            //记录可重新执行的物体，记录后可以对它做撤销等操作，比如物体的财产变动等
            Undo.RecordObject(this, "Node Tree(CreateNodeTree)");
            //创建一个ScriptableObject实例
            CustomNode node = ScriptableObject.CreateInstance(type) as CustomNode;
            node.tree=this;
            node.current = node;
            node.name = type.Name;
            node.guid = System.Guid.NewGuid().ToString();
            nodes.Add(node);
            if (!Application.isPlaying)
            {
                //将 objectToAdd 添加到 path 下的现有资源中。
                AssetDatabase.AddObjectToAsset(node, this);
            }
            //注册物体的创建后可撤销创建操作
            Undo.RegisterCreatedObjectUndo(node, "Node Tree(CreateNodeTree)");
            //迅速保存用代码创建出来的资源文件
            AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public CustomNode DeleteNode(CustomNode node)
        {
            Undo.RecordObject(this, "Node Tree(DeleteNodeTree)");
            nodes.Remove(node);
            Groups.ForEach(group=>{if(group.node_guids.Contains(node.guid)) group.node_guids.Remove(node.guid);});
            //AssetDatabase.RemoveObjectFromAsset(node);
            //快速删除一个资源文件并使其可以被撤销操作重新创建
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
            return node;
        }

        /// <summary>
        /// 通过GUID寻找树中的节点
        /// </summary>
        /// <param name="findGuid"></param>
        /// <returns></returns>
        public CustomNode FindNodeByGuid(string findGuid){
            return nodes.Where(n=>n.guid.Equals(findGuid)).FirstOrDefault();
        }

        /// <summary>
        /// 移除父节点的指定子节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        // public void RemoveChild(CustomNode parent, CustomNode child)
        // {
        //     Undo.RecordObject(parent, "Node Tree(RemoveNodeTree)");
        //     if (parent is SingleNode)
        //     {
        //         (parent as SingleNode).child = null;
        //     }
        //     else
        //     {
        //         (parent as CompositeNode).RemoveChild(child);
        //     }
        //     //由于使用代码修改了资源文件的数据，若想保存修改，就得将被修改的资源文件设置为脏
        //     EditorUtility.SetDirty(parent);
        // }

        /// <summary>
        /// 为指定父节点添加子节点
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        // public void AddChild(CustomNode parent, CustomNode child)
        // {
        //     Undo.RecordObject(parent, "Node Tree(AddNodeTree)");
        //     if (parent is SingleNode)
        //     {
        //         (parent as SingleNode).child = child;
        //     }
        //     else
        //         (parent as CompositeNode).AddChild(child);
        //     EditorUtility.SetDirty(parent);
        // }

        /// <summary>
        /// 获取分支节点的所有子节点
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>子节点List</returns>
        // public List<CustomNode> GetChldren(CustomNode parent)
        // {
        //     return (parent as CompositeNode).children;
        // }

        public virtual CustomGraphGroup AddGroup(System.Type groupType)
		{
			if (typeof(CustomGraphGroup).IsAssignableFrom(groupType))
			{
				Debug.Log("Create Group");
				Undo.RecordObject(this, "Node Tree(CreateNodeTree)");
				CustomGraphGroup result = Activator.CreateInstance(groupType) as CustomGraphGroup;
                result.group_guid=System.Guid.NewGuid().ToString();
				Groups.Add(result);
                EditorUtility.SetDirty(this);
				return result;
			}
			return null;
		}

		public virtual CustomGraphGroup RemoveGroup(CustomGraphGroup group)
        {
            Undo.RecordObject(this, "Node Tree(DeleteNodeTree)");
            Groups.Remove(group);
            //AssetDatabase.RemoveObjectFromAsset(node);
            //快速删除一个资源文件并使其可以被撤销操作重新创建
            EditorUtility.SetDirty(this);
            return group;
        }

#endif
    }
}


