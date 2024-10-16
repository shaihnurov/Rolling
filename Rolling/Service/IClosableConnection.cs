using System.Threading.Tasks;

namespace Rolling.Service;

public interface IClosableConnection
{
    Task CloseConnectionAsync();
}