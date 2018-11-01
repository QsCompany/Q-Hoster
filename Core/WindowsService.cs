using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.ServiceProcess;
using System.Security.Principal;
using System.Runtime.InteropServices;
//[assembly: SuppressIldasm]

//[assembly: CompilerGenerated]
[module: UnverifiableCode]


namespace QServer.Core
{
    public enum StartupTypeOptions : uint
    {
        BootStart = 0,      //A device driver started by the system loader. This value is valid only for driver services.
        SystemStart = 1,    //A device driver started by the IoInitSystem function. This value is valid only for driver services.
        Automatic = 2,      //A service started automatically by the service control manager during system startup.
        Manual = 3,         //A service started by the service control manager when a process calls the StartService function.
        Disabled = 4        //A service that cannot be started. Attempts to start the service result in the error code ERROR_SERVICE_DISABLED.
    }
    public class WindowsService
    {

        public static bool IsUserAdministrator
        {
            get
            {
                bool isAdmin;
                try
                {
                    WindowsIdentity user = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(user);
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (UnauthorizedAccessException ex)
                {
                    isAdmin = false;
                }
                catch (Exception ex)
                {
                    isAdmin = false;
                }
                return isAdmin;
            }
        }
        public static void ChangeServiceStartType(string serviceName, StartupTypeOptions startType)
        {
            //Obtain a handle to the service control manager database
            IntPtr scmHandle = OpenSCManager(null, null, SC_MANAGER_CONNECT);
            if (scmHandle == IntPtr.Zero)
            {
                throw new Exception("Failed to obtain a handle to the service control manager database.");
            }

            //Obtain a handle to the specified windows service
            IntPtr serviceHandle = OpenService(scmHandle, serviceName, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);
            if (serviceHandle == IntPtr.Zero)
            {
                throw new Exception(string.Format("Failed to obtain a handle to service \"{0}\".", serviceName));
            }

            bool changeServiceSuccess = ChangeServiceConfig(serviceHandle, SERVICE_NO_CHANGE, (uint)startType, SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null);

            if (!changeServiceSuccess)
            {
                string msg = string.Format("Failed to update service configuration for service \"{0}\". ChangeServiceConfig returned error {1}.", serviceName, Marshal.GetLastWin32Error().ToString());
                throw new Exception(msg);
            }

            //Clean up
            if (scmHandle != IntPtr.Zero) CloseServiceHandle(scmHandle);
            if (serviceHandle != IntPtr.Zero) CloseServiceHandle(serviceHandle);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            UInt32 nServiceType,
            UInt32 nStartType,
            UInt32 nErrorControl,
            String lpBinaryPathName,
            String lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [InAttribute] char[] lpDependencies,
            String lpServiceStartName,
            String lpPassword,
            String lpDisplayName);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        private static extern int CloseServiceHandle(IntPtr hSCObject);

        private const uint SC_MANAGER_CONNECT = 0x0001;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;

        public static bool StartService(string serviceName = "MySQL", int timeoutMilliseconds = 12000)
        {
            if(false)
            if (!IsUserAdministrator)
            {
                global::Server.Program.Exit(null, true, true);
                return false;
            }
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            var ct = DateTime.Now + timeout;
            deb:
            if (DateTime.Now >= ct) return false;
            ServiceController service = new ServiceController(serviceName);
            
            //switch (service.StartType)
            //{
            //    case ServiceStartMode.Disabled:
            //        ChangeServiceStartType(serviceName, StartupTypeOptions.Automatic);
            //        break;
            //}

            switch (service.Status)
            {

                case ServiceControllerStatus.Running:
                    return true;
                case ServiceControllerStatus.ContinuePending:
                case ServiceControllerStatus.StartPending:
                    Thread.Sleep(1000);
                    goto deb;
                default:
                    try
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                        return true;
                    }
                    catch (Exception e)
                    {
                        
                        goto deb;
                    }

            }
        }
    }
}
