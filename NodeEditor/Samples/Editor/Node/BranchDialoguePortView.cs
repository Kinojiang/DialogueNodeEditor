using CustomNodeTree;
using CustomNodeTree.Core;
using CustomNodeTreeEditor;
using UnityEngine.UIElements;


[CustomPortView(typeof(BranchDialoguePort))]
public class BranchDialoguePortView : CustomportView
{
    public override void CreateView(CustomPort port)
    {
        Label field = new Label(port.Name);
		field.style.width = 100;
		Add(field);
    }
}
