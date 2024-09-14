using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="DialogueEventConfigure",menuName ="NodeEditor/DialogueConfigure")]
public class DialogueEventSO : ScriptableObject
{
    public Action DialogueAction;
}
