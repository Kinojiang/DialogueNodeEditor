using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
namespace CustomNodeTree.Core
{
    public abstract class CompositeNode : CustomNode
    {
        private List<OptionNode> children = new List<OptionNode>();

        protected virtual List<OptionNode> GetOutoutNode(){
            children.Clear();
            if(Ports.Count>0){
                var outputPorts=Ports.Where(p=>p.Direction==CustomPort.PortDirection.Output);
                foreach(var port in outputPorts){
                    port.Connections.ForEach(
                        con=>{
                            var node=tree.FindNodeByGuid(con.node_guid) as OptionNode;
                            if(node!=null && !children.Contains(node)){
                                children.Add(node);
                            }
                        }
                    );
                }
            }
            return children;
        }
        
    }

}

