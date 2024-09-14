using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorViewer : VisualElement
{
    public new class UxmlFactory : UxmlFactory<InspectorViewer,VisualElement.UxmlTraits>{}

    Editor editor;
    internal void UpdateSelection(NodeView nodeView){
        Clear();
        //快速销毁已有的editor
        UnityEngine.Object.DestroyImmediate(editor);
        //为 targetObject 或 targetObjects 创建自定义编辑器。
        editor=Editor.CreateEditor(nodeView.node);
        IMGUIContainer container=new IMGUIContainer(()=>{
            if(editor.target){
                //实现此函数以创建自定义检视面板,不实现就是默认检视面板
                editor.OnInspectorGUI();
            }
        });
        Add(container);
    }
}
