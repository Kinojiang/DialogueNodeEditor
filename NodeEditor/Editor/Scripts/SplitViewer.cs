using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SplitViewer : TwoPaneSplitView
{
    public new class UxmlFactory:UxmlFactory<SplitViewer,TwoPaneSplitView.UxmlTraits>{}
}
