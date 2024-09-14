using CustomNodeTree;
using CustomNodeTree.Core;
using UnityEngine.UIElements;

namespace CustomNodeTreeEditor
{
	[CustomPortView(typeof(CustomPort))]
    public class CustomGraphLabelPortView : CustomportView
    {
		public override void CreateView(CustomPort port)
		{
			Label field = new Label(port.Name);
			Add(field);
		}
	}
}

