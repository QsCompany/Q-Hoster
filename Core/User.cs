using System;
using System.Net;
using models;

namespace Server
{
    [QServer.Core.HosteableObject(typeof(global::Api.User), null)]
    public class  User
    {
        public readonly string UserName, Password;
        private string id, lid;
        public string lCurrentId => lid;
        public string CurrentId { get => id;
            set { if (value == id)return; lid = id; id = value; } }
        private bool _isBlocked;

        public byte[] Key = new byte[32] { 234, 23, 196, 234, 69, 238, 92, 244, 50, 110, 70, 181, 109, 139, 252, 209, 146, 174, 40, 140, 129, 41, 58, 89, 102, 193, 99, 194, 178, 192, 239, 152 };

        public bool IsBlocked
        {
            get => _isBlocked;
            set => _isBlocked = value;
        }
        
        public IPAddress Address;

        public User(Login l,bool f)
        {
            Login = l;
            UserName = l.Username;
            Password = l.Pwd;
            IsBlocked = false;
            IsAgent = l is Agent;
            IsAdmin = IsAgent ? ((Agent)l).Permission == AgentPermissions.Admin : false;
        }
        public AgentPermissions Permission
        {
            get
            {
                if (IsAgent) return AgentPermissions.None;
                var t = (Agent)Login;
                if (t == null) return AgentPermissions.None;
                return t.Permission;
            }
        }
        
        public bool Check(RequestArgs serviceArgs)
        {
            return true;
        }
        
        public readonly Login Login;
        public bool IsLogged;
        public bool AllowSigninById = true;
		internal DateTime LastAccess;

        public Agent Agent => Login as Agent;

        public bool IsAdmin { get; }
        public bool IsAgent { get; }
        public string Identification { get; set; }

        public Client Client => Login.Client;
    }
}
