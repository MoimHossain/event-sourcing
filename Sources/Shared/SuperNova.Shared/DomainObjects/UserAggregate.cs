

using SuperNova.Shared.Messaging.Events.Users;
using SuperNova.Shared.Supports;
using System;

namespace SuperNova.Shared.DomainObjects
{
    public class UserAggregate : AggregateRoot
    {
        private string _userName;
        private string _emailAddress;
        private Guid _userId;
        private bool _blocked;

        #region Accept commands
        public void RegisterNew(string userName, string emailAddress)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Ensure.ArgumentNotNullOrWhiteSpace(emailAddress, nameof(emailAddress));

            ApplyChange(new UserRegistered
            {
                AggregateId = Guid.NewGuid(),
                Email = emailAddress,
                UserName = userName                
            });
        }

        public void BlockUser(Guid userId)
        {            
            ApplyChange(new UserBlocked
            {
                AggregateId = userId
            });
        }

        public void RenameUser(Guid userId, string name)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            ApplyChange(new UserNameChanged
            {
                AggregateId = userId,
                NewName = name
            });
        }
        #endregion

        #region Apply events
        private void Apply(UserRegistered e)
        {
            this._userId = e.AggregateId;
            this._userName = e.UserName;
            this._emailAddress = e.Email;            
        }

        private void Apply(UserBlocked e)
        {
            this._blocked = true;
        }

        private void Apply(UserNameChanged e)
        {
            this._userName = e.NewName;
        }
        #endregion
    }
}
