using Server;
using System;
using System.Reflection;

namespace QServer.Core
{

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ServiceAttribute : Attribute
    {
        public static void LoadAssembly(System.Reflection.Assembly assembly, Core.Server server)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attrs = type.GetCustomAttributes(typeof(ServiceAttribute), false);
                if (attrs.Length > 0)
                    server.AddService((Service)Activator.CreateInstance(type));
            }
        }
    }
    [AttributeUsage(AttributeTargets.Module| AttributeTargets.Class| AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
    sealed class HosteableObjectAttribute : Attribute
    {
        public Type ServiceType { get; set; }
        public Type SerializerType { get; set; }

        public void RegisterTo(Core.Server server)
        {
            if (ServiceType != null)
                server.AddService((Service)Activator.CreateInstance(ServiceType));
            if (SerializerType != null)
                server.AddSerializer((Typeserializer)Activator.CreateInstance(SerializerType));
        }
        public HosteableObjectAttribute(Type ServiceType, Type SerializerType=null)
        {
            if (ServiceType != null && !ServiceType.IsSubclassOf(typeof(Service))) throw null;
            if (SerializerType != null && !SerializerType.IsSubclassOf(typeof(Typeserializer))) throw null;

            this.ServiceType = ServiceType;
            this.SerializerType = SerializerType;
        }
        public static void LoadAssembly(System.Reflection.Assembly assembly, Core.Server server)
        {
            ServiceAttribute.LoadAssembly(assembly, server);
            foreach(HosteableObjectAttribute attr in assembly.GetCustomAttributes(typeof(HosteableObjectAttribute),false))
                attr.RegisterTo(server);
            foreach (var module in assembly.GetModules())
                foreach (HosteableObjectAttribute attr in module.GetCustomAttributes(typeof(HosteableObjectAttribute),false))
                    attr.RegisterTo(server);                
            
            foreach (Type type in assembly.GetTypes())
            {
                var attrs = type.GetCustomAttributes(typeof(HosteableObjectAttribute), false);
                if (attrs.Length > 0)
                {
                    var attr = attrs[0] as HosteableObjectAttribute;
                    attr.RegisterTo(server);
                }
            }
        }

        
    }
}
