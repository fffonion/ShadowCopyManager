using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace ShadowCopyManager

{
    // From https://gist.github.com/jborean93/f60da33b08f8e1d5e0ef545b0a4698a0/7657ad8e24c595db8ad563cc6dbc434c2bbc01f6
    //  and https://github.com/Joachim-Otahal/Windows-ShadowCopy/blob/main/ShadowCopyConfig.ps1
    public class VssTool
    {
        private const uint FSCTL_SRV_ENUMERATE_SNAPSHOTS = 0x00144064;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [StructLayout(LayoutKind.Sequential)]
        private struct IoStatusBlock
        {
            public uint Status;
            public uint Information;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NtTransData
        {
            public uint NumberOfSnapShots;
            public uint NumberOfSnapShotsReturned;
            public uint SnapShotArraySize;
        }

        private class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern SafeFileHandle CreateFileW(
                string lpFileName,
                FileSystemRights dwDesiredAccess,
                FileShare dwShareMode,
                IntPtr lpSecurityAttributes,
                FileMode dwCreationDisposition,
                uint dwFlagsAndAttributes,
                IntPtr hTemplateFile);

            [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint NtFsControlFile(
                SafeFileHandle hDevice,
                IntPtr Event,
                IntPtr ApcRoutine,
                IntPtr ApcContext,
                ref IoStatusBlock IoStatusBlock,
                uint FsControlCode,
                IntPtr InputBuffer,
                uint InputBufferLength,
                IntPtr OutputBuffer,
                uint OutputBufferLength);

            [DllImport("ntdll.dll")]
            public static extern uint RtlNtStatusToDosError(uint Status);
        }

        public static void EnableUNCPathFor(string path)
        {
            using (var handle = NativeMethods.CreateFileW(
                path,
                FileSystemRights.ListDirectory | FileSystemRights.ReadAttributes | FileSystemRights.Synchronize,
                FileShare.ReadWrite,
                IntPtr.Zero,
                FileMode.Open,
                FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero))
            {
                if (handle.IsInvalid)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Set the initial buffer size to the size of NT_Trans_Data + 2 chars. We do this so we can get the actual buffer
                // size that is contained in the NT_Trans_Data struct. A char is 2 bytes (UTF-16) and we expect 2 of them
                var transDataSize = Marshal.SizeOf<NtTransData>();
                var bufferSize = transDataSize + 4;
                var outBuffer = Marshal.AllocHGlobal(bufferSize);

                var ioBlock = new IoStatusBlock();

                // Actually triggering, after that the access works.
                var result = NativeMethods.NtFsControlFile(
                       handle,
                       IntPtr.Zero,
                       IntPtr.Zero,
                       IntPtr.Zero,
                       ref ioBlock,
                       FSCTL_SRV_ENUMERATE_SNAPSHOTS,
                       IntPtr.Zero,
                       0,
                       outBuffer,
                       (uint)bufferSize);

                if (result != 0)
                {
                    var win32Error = NativeMethods.RtlNtStatusToDosError(result);
                    throw new Win32Exception((int)win32Error);
                }

                Marshal.FreeHGlobal(outBuffer);
            }
        }
    }
}