using System;
using System.Threading.Tasks;
using Weave.ViewModels;

namespace Weave.Services.Startup
{
    public class Transition_GetRandomId : IState
    {
        UserInfo user;

        public enum State
        {
            Success,
            Fail
        }

        public State? CurrentState { get; private set; }

        public Transition_GetRandomId(UserInfo user)
        {
            this.user = user;
        }

        public Task Transition()
        {
            var newId = Guid.NewGuid();
            user.Id = newId;
            CurrentState = State.Success;

            return Task.FromResult<object>(null);
        }
    }
}
