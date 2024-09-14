using CustomNodeTree;
using CustomNodeTree.Core;
using UnityEngine;


//该类默认使用什么类型的CustomNode
[DefaultNodeType(typeof(CustomNode))]
[CreateAssetMenu(fileName ="DialogueTree",menuName ="NodeEditor/DialogueTree")]
public class DialogueTree : NodeTree
{
    public override void OnTreeStart()
    {
        base.OnTreeStart();
        GameObject.FindObjectOfType<DialogueManager>().StartDialogue();
    }

    public override void OnTreeEnd()
    {
        base.OnTreeEnd();
        GameObject.FindObjectOfType<DialogueManager>().EndDialogue();
    }
}
