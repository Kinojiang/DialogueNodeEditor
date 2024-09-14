using System.Linq;

namespace CustomNodeTree.Core
{
    public abstract class SingleNode : CustomNode
    {
        protected virtual CustomNode GetNextNode(){
            if(Ports.Count>0){
                CustomPort port = Ports.Where(p=>p.Direction==CustomPort.PortDirection.Output).FirstOrDefault();
                if(port!=null&&port.Connections.Count>0){
                    return tree.FindNodeByGuid(port.Connections[0].node_guid);
                }
            }
            return null;
        }
    }
}


