using System.Linq;
using CustomNodeTree;
using CustomNodeTree.Core;
using UnityEngine;

[NodeCategory("Option")]
[NodeName("Option")]
[CustomNodeStyle("OptionNodeView")]
[NodePortAggregate(NodePortAggregateAttribute.PortAggregate.Single, NodePortAggregateAttribute.PortAggregate.Single)]
[PortCapacity(PortCapacityAttribute.Capacity.Single, PortCapacityAttribute.Capacity.Single)]
public class OptionNode : SingleNode
{
    [TextArea]
    public string content;
    public DialogueEventSO configure;

    private DialogueManager dialogueManager;
    public override CustomNode LogicUpdate()
    {
        state = State.Waiting;
        var node = GetNextNode();
        if (node != null)
        {
            node.state = CustomNode.State.Running;
            return node;
        }
        return this;
    }

    public override void OnStart()
    {
        dialogueManager = GameObject.FindObjectOfType<DialogueManager>();
        //configure.DialogueAction();
    }

    public override void OnStop()
    {
        if (GetNextNode() == null)
        {
            dialogueManager.EndDialogue();
        }
    }
}
