using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using CustomNodeTree.Core;

namespace CustomNodeTreeEditor
{
    public class GraphGroupView : Group
    {
        public CustomGraphGroup Group => group;
        private CustomGraphGroup group;

        public void InitGroup(CustomGraphGroup group)
        {
            this.group = group;
            viewDataKey = group.group_guid; ;
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);

            foreach (var element in elements)
            {
                CustomNode node = element.userData as CustomNode;
                if (node != null && !group.node_guids.Contains(node.guid))
                {
                    Debug.Log(node.tree);
                    Undo.RecordObject(node.tree,"Add Element to Group");
                    group.node_guids.Add(node.guid);
                }
                //Debug.Log((element.parent as VisualGraphGroupView).title);
            }
            //EditorUtility.SetDirty(group);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            // foreach (var element in elements)
            // {
            //     CustomNode node = element.userData as CustomNode;
            //     if (node != null)
            //         group.node_guids.Remove(node.guid);
            // }
            // EditorUtility.SetDirty(group);
        }

        protected override void OnGroupRenamed(string oldName, string newName)
        {
            base.OnGroupRenamed(oldName, newName);
            group.title = newName;
            //EditorUtility.SetDirty(group);
        }

        /// <summary>
        /// 手动移除分组中的元素
        /// </summary>
        /// <param name="element"></param>
        public void ManualRemoveElement(GraphElement element)
        {
            RemoveElement(element);
            CustomNode node = element.userData as CustomNode;
            if (node != null){
                Undo.RecordObject(node.tree,"Remove Element From Group");
                group.node_guids.Remove(node.guid);
            }
            //EditorUtility.SetDirty(group);
        }
    }
}