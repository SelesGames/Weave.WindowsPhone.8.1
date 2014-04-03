using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.SavedState;
using Weave.ViewModels;

namespace Weave.Services.Startup
{
    public class StartupIdentityStateMachine
    {
        UserInfo user;
        PermanentState permState;
        IState currentState;
        bool isComplete = false;

        public enum State
        {
            UserExists,
            NoUserFound
        }

        public State? FinalState { get; private set; }

        public StartupIdentityStateMachine(UserInfo user, PermanentState permState)
        {
            this.user = user;
            this.permState = permState;
        }

        public async Task Begin()
        {
            Init();

            while (!isComplete)
            {
                await ChooseNextState();
            }
        }




        #region Transition functions

        void Init()
        {
            if (user.Id != Guid.Empty)
            {
                currentState = new Transition_GetUserById(user, permState);
            }
            else
            {
                //currentState = new Transition_GetMicrosoftId(user);
                currentState = new Transition_GetRandomId(user);
            }
        }

        Task ChooseNextState()
        {
            //if (currentState is Transition_GetMicrosoftId)
            //{
            //    return TransitionGetMicrosoftId();
            //}

            if (currentState is Transition_GetRandomId)
            {
                return TransitionGetRandomId();
            }

            //else if (currentState is Transition_MigrateLocalFeedsToCloud)
            //{
            //    return TransitionRecoverLostFeeds();
            //}

            else if (currentState is Transition_GetUserById)
            {
                return TransitionGetUserById();
            }

            else throw new Exception("unidentified transition state");
        }

        //async Task TransitionGetMicrosoftId()
        //{
        //    var state = (Transition_GetMicrosoftId)currentState;
        //    await state.Transition();

        //    if (state.CurrentState == Transition_GetMicrosoftId.State.Success)
        //        currentState = new Transition_GetUserById(user);

        //    else if (state.CurrentState == Transition_GetMicrosoftId.State.Fail)
        //        currentState = new Transition_GetRandomId(user);
        //}

        async Task TransitionGetRandomId()
        {
            var state = (Transition_GetRandomId)currentState;
            await state.Transition();

            if (state.CurrentState == Transition_GetRandomId.State.Success)
                currentState = new Transition_GetUserById(user, permState);
                //currentState = new Transition_MigrateLocalFeedsToCloud(user, permState);

            else if (state.CurrentState == Transition_GetRandomId.State.Fail)
                throw new CriticalApplicationException();
        }
  
        //async Task TransitionRecoverLostFeeds()
        //{
        //    var state = (Transition_MigrateLocalFeedsToCloud)currentState;
        //    await state.Transition();

        //    if (state.CurrentState == Transition_MigrateLocalFeedsToCloud.State.Unnecessary)
        //        currentState = new Transition_GetUserById(user, permState);

        //    else if (state.CurrentState == Transition_MigrateLocalFeedsToCloud.State.FeedsMigrated)
        //        currentState = new Transition_GetUserById(user, permState);

        //    else if (state.CurrentState == Transition_MigrateLocalFeedsToCloud.State.Fail)
        //    {
        //        MessageBox.Show("Unable to import your existing feeds.  Please ensure you have an internet connection, and relaunch the app to try again.", "Whoops!", MessageBoxButton.OK);
        //        Application.Current.Terminate();
        //    }
        //}

        async Task TransitionGetUserById()
        {
            var state = (Transition_GetUserById)currentState;
            await state.Transition();

            if (state.CurrentState == Transition_GetUserById.State.Success)
            {
                FinalState = State.UserExists;
            }

            else if (state.CurrentState == Transition_GetUserById.State.Fail)
            {
                FinalState = State.NoUserFound;
            }

            currentState = null;
            isComplete = true;
        }

        #endregion
    }
}
