using CustomNodeTree;
using CustomNodeTree.Core;
using CustomNodeTreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomNodeView(typeof(NormalDialogue))]
public class NormalDialogueView : NodeView
{
    [HideInInspector] public override bool ShowNodeProperties => true;

    public override void DrawNode()
    {
        base.DrawNode();

        VisualElement node_data = new VisualElement();
        node_data.style.backgroundColor = Color.blue;


        Label example = new Label("Custom Node");
        node_data.Add(example);

        mainContainer.Add(node_data);
    }
}
