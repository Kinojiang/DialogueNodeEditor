using CustomNodeTree;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomNodeView(typeof(OptionNode))]
public class OptionNodeView : NodeView
{
    public override bool ShowNodeProperties => false;

    public override void DrawNode()
    {
        base.DrawNode();
        this.style.width=default_size.x;
        //添加折叠
        Foldout mainFoldout=new Foldout();
        mainFoldout.name="mainFoldout";
        TextField content = new TextField();
        content.value=(userData as OptionNode).content;
        // 监听输入变化
        content.RegisterValueChangedCallback(evt =>
        {
            (userData as OptionNode).content = evt.newValue;  // 成功转换，更新字段
            EditorUtility.SetDirty(userData as OptionNode);

        });
        mainFoldout.Add(content);

        ObjectField objectField=new ObjectField();
        objectField.objectType=typeof(DialogueEventSO);
        objectField.value=(userData as OptionNode).configure;
        objectField.RegisterValueChangedCallback(evt=>{
            (userData as OptionNode).configure=evt.newValue as DialogueEventSO;
            EditorUtility.SetDirty(userData as OptionNode);
        });
        mainFoldout.Add(objectField);

        mainContainer.Add(mainFoldout);
    }
}
