using UnityEditor.Experimental.GraphView;
using CustomNodeTreeEditor;
using CustomNodeTree;

[CustomNodeView(typeof(StartingNode))]
public class StartingNodeView : NodeView
{
    public override bool ShowNodeProperties => false;

        public override Capabilities SetCapabilities(Capabilities capabilities)
        {
            capabilities &= ~UnityEditor.Experimental.GraphView.Capabilities.Movable;
            capabilities &= ~UnityEditor.Experimental.GraphView.Capabilities.Deletable;
            return capabilities;
        }
}
