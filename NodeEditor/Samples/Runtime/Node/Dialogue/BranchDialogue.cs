using System.Collections;
using System.Collections.Generic;
using CustomNodeTree;
using CustomNodeTree.Core;
using UnityEngine;

[NodePortAggregate(NodePortAggregateAttribute.PortAggregate.Multiple, NodePortAggregateAttribute.PortAggregate.Multiple)]
[PortCapacity(PortCapacityAttribute.Capacity.Multi, PortCapacityAttribute.Capacity.Multi)]
[DefaultPortType(typeof(BranchDialoguePort))]
[NodeCategory("BranchNode/BranchDialogue")]
public class BranchDialogue : CompositeNode
{
    public string speakerName;
    public Sprite speakerAvatar;
    [TextArea]
    public string content;

    public DialogueEventSO configure;
    public AudioClip sound;

    private List<OptionNode> optionNodes;
    private int choiceIndex;

    private bool isSelected;
    private DialogueManager dialogueManager;

    public override CustomNode LogicUpdate()
    {
        if(isSelected){
            state=State.Waiting;
            isSelected=false;
            if(optionNodes.Count>0){
                optionNodes[choiceIndex].state=State.Running;
                return optionNodes[choiceIndex];
            }
        }
        return this;
    }

    public override void OnStart()
    {
        optionNodes=GetOutoutNode();
        dialogueManager = GameObject.FindObjectOfType<DialogueManager>();
        dialogueManager.UpdateDialogueInfo(speakerAvatar,content,speakerName);
        List<UnityEngine.UI.Button> buttons=dialogueManager.UpdateBranchPanel(optionNodes.Count);
        for(int i=0;i<buttons.Count;i++){
            buttons[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text=optionNodes[i].content;
            int currentIndex=buttons[i].transform.GetSiblingIndex();
            buttons[i].onClick.AddListener(()=>{
                choiceIndex = currentIndex;
                isSelected=true;
            });
        }
    }

    public override void OnStop()
    {
        dialogueManager.DestroyBranchPanelChldren();
        if(optionNodes.Count<=0){
            dialogueManager.EndDialogue();
        }
    }
}