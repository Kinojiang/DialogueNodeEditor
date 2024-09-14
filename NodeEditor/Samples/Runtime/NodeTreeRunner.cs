using System;
using System.Collections;
using System.Collections.Generic;
using CustomNodeTree.Core;
using UnityEngine;

public class NodeTreeRunner : MonoBehaviour
{
    public NodeTree tree;


    private void Awake() {
        tree.InitTree();
        //StartCoroutine(TestIE());
        //Debug.Log("Awake");
    }

    // IEnumerator TestIE(){
    //     Debug.Log("StartCoroutine");
    //     Time.timeScale=0;
    //     yield return new WaitForSecondsRealtime(2f);
    //     Time.timeScale=1;
    //     Debug.Log("OverCoroutine");
    // }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            tree.OnTreeStart();
        }

        //若符合条件，对节点树进行帧调用
        if(tree!=null&&tree.treeState==CustomNode.State.Running){
            tree.Update();
        }

        if(Input.GetKeyDown(KeyCode.D)){
            tree.OnTreeEnd();
        }

        Debug.Log(Application.IsPlaying(this));
    }
}

[Serializable]
public class SerTest:ScriptableObject{
    public int i=0;
}

public class Test{
    public float a=2.5f;
}