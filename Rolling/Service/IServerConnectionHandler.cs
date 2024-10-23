using System.Threading.Tasks;

namespace Rolling.Service;

public interface IServerConnectionHandler
{
    public Task ConnectToSignalR();
}