using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNova.Shared.Messaging.Events.Users
{
    public class UserNameChanged : EventBase
    {

        public string NewName { get; set; }
    }
}
