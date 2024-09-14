using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using CustomNodeTree.Core;
// using DG.Tweening;
// using DG.Tweening.Core;
// using DG.Tweening.Plugins.Options;

public class DialogueManager : MonoBehaviour
{
    public Image speakerAvatar;
    public Text content;
    public Text speakerName;

    public GameObject branchPanel;
    public Button branchButton;

    private bool isDialoguing;
    public bool IsDialoguing=>isDialoguing;

    public void UpdateDialogueInfo(Sprite sprite,string contentInfo,string name){
        if(sprite==null) speakerAvatar.gameObject.SetActive(false);
        else speakerAvatar.gameObject.SetActive(true);
        speakerAvatar.sprite=sprite;
        speakerAvatar.SetNativeSize();
        StartCoroutine(DisplayDialogue(content,contentInfo,1f));
        speakerName.text=name;
        //isDialoguing=true;
        //content.text="";
        //TweenerCore<string, string, StringOptions> coroutine = content.DOText(contentInfo,1f);
        //coroutine.OnComplete(()=>isDialoguing=false);
    }

    IEnumerator DisplayDialogue(Text text,string targetValue,float duration)
    {
        isDialoguing=true;
        yield return StartCoroutine(text.AppearString(targetValue,0.5f));
        isDialoguing=false;
    }

    public List<Button> UpdateBranchPanel(int value){
        List<Button> newButtonList=new List<Button>();
        for(int i=0;i<value;i++){
            Button newButton=Instantiate(branchButton,branchPanel.transform);
            newButtonList.Add(newButton);
        }
        return newButtonList;
    }

    //清除现在所有的分支选项
    public void DestroyBranchPanelChldren(){
        for(int i=0;i<branchPanel.transform.childCount;i++){
            Destroy(branchPanel.transform.GetChild(i).gameObject);
        }
    }

    public void StartDialogue(){
        this.transform.Find("DialogueUI").gameObject.SetActive(true);
    }

    public void EndDialogue(){
        isDialoguing=false;
        this.transform.Find("DialogueUI").gameObject.SetActive(false);
    }
}
