using CustomNodeTree.Core;
using UnityEngine.UIElements;

namespace CustomNodeTreeEditor
{
    /// <summary>
    /// 自定义视图端口，用来设置端口样式
    /// </summary>
    
    public abstract class CustomportView : VisualElement
    {
        public abstract void CreateView(CustomPort port);
    }
}