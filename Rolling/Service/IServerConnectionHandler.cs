using System.Threading.Tasks;

namespace Rolling.Service;

public interface IServerConnectionHandler
{
    public void ConnectToSignalR();
    public Task StopConnection();
}