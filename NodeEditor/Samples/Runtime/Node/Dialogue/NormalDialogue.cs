using CustomNodeTree;
using CustomNodeTree.Core;
using UnityEngine;

//普通对话节点，只返回一种情况的对话内容
// 限制输入输出端可存在的端口数量
[NodePortAggregate(NodePortAggregateAttribute.PortAggregate.Single, NodePortAggregateAttribute.PortAggregate.Single)]
// 限制端口的可连接数量
[PortCapacity(PortCapacityAttribute.Capacity.Multi, PortCapacityAttribute.Capacity.Single)]
[NodeCategory("SingleNode/NormalDialogue")]
[CustomNodeStyle("NormalDialogueViewStyle")]
public class NormalDialogue : SingleNode
{
    public string speakerName;
    public Sprite speakerAvatar;
    [TextArea]
    public string content;

    public DialogueEventSO configure;
    public AudioClip sound;

    private DialogueManager dialogueManager;
    public override CustomNode LogicUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space)&&!dialogueManager.IsDialoguing){
            state=State.Waiting;
            var node=GetNextNode();
            if(node!=null){
                node.state=CustomNode.State.Running;
                return node;
            }
        }
        return this;
    }

    public override void OnStart()
    {
        dialogueManager = GameObject.FindObjectOfType<DialogueManager>();
        dialogueManager.UpdateDialogueInfo(speakerAvatar,content,speakerName);
    }

    public override void OnStop()
    {
        if(GetNextNode()==null){
            dialogueManager.EndDialogue();
        }
    }
}
