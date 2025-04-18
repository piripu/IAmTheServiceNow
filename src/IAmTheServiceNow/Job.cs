using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.JobObjects;

namespace IAmTheServiceNow;

public sealed class Job : IDisposable
{
    private SafeFileHandle _jobHandle;
    
    public unsafe Job(string? name)
    {
        var securityAttributes = new SECURITY_ATTRIBUTES
        {
            lpSecurityDescriptor = IntPtr.Zero.ToPointer(),
            nLength = (uint)Marshal.SizeOf<SECURITY_ATTRIBUTES>(),
        };

        _jobHandle = PInvoke.CreateJobObject(securityAttributes, name);
        
        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            }
        };

        var infoClass = JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation;
        var infoLength = (uint)Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
        
        if (!PInvoke.SetInformationJobObject(_jobHandle, infoClass, &info, infoLength))
        {
            throw new Exception($"Unable to set information.  Error: {Marshal.GetLastWin32Error()}");
        }
    }
    
    

    public void Terminate()
    {
        PInvoke.TerminateJobObject(_jobHandle, 1);
        _jobHandle = new SafeFileHandle(IntPtr.Zero, false);
    }

    public void AddProcess(Process process)
    {
        var addedRef = false;
        try
        {
            _jobHandle.DangerousAddRef(ref addedRef);
            if (!PInvoke.AssignProcessToJobObject((HANDLE)_jobHandle.DangerousGetHandle(), (HANDLE)process.Handle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        finally
        {
            if (addedRef)
            {
                _jobHandle.DangerousRelease();
            }
        }
    }

    public void AddProcess(int processId)
    {
        AddProcess(Process.GetProcessById(processId));
    }
    
    public void Dispose()
    {
        _jobHandle.Dispose();
    }
}