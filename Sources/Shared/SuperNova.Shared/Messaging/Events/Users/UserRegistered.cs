using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.Messaging.Events.Users
{
    public class UserRegistered : EventBase
    {
        public UserRegistered()
        {

        }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
