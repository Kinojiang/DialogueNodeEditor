using CustomNodeTree;
using CustomNodeTree.Core;
using CustomNodeTreeEditor;
using UnityEngine.UIElements;

public class CustomGraphDefaultPortView : CustomportView
{
    public override void CreateView(CustomPort port)
    {
        TextField leftField = new TextField();
			leftField.value = port.Name;
			leftField.style.width = 100;
			leftField.RegisterCallback<ChangeEvent<string>>(
				(evt) =>
				{
					if (string.IsNullOrEmpty(evt.newValue) == false)
					{
						port.Name = evt.newValue;
					}
				}
			);
			Add(leftField);
    }
}
