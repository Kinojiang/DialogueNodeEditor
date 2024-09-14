using UnityEngine;
using CustomNodeTree;
using CustomNodeTree.Core;

[NodeName("Start")]
[NodePortAggregate(NodePortAggregateAttribute.PortAggregate.None, NodePortAggregateAttribute.PortAggregate.Single)]
public class StartingNode : CustomNode
{
    public override CustomNode LogicUpdate()
    {
        state=CustomNode.State.Waiting;
        if(Ports.Count>0&&Ports[0].Connections.Count>0){
            var nextNode=tree.FindNodeByGuid(Ports[0].Connections[0].node_guid);
            nextNode.state=CustomNode.State.Running;
            return nextNode ;
        }
        return this;
    }

    public override void OnStart()
    {
       Debug.Log("EnterStart");
    }

    public override void OnStop()
    {
        Debug.Log("EndStart");
    }
}
