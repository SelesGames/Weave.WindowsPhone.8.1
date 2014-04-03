using System.Threading.Tasks;

namespace Weave.Services.Startup
{
    public interface IState
    {
        Task Transition();
    }
}
