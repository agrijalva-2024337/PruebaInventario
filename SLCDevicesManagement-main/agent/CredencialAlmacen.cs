using System.Runtime.InteropServices;
using System.Text;

namespace SLCDM.Agent;

/// <summary>
/// Guarda el token en el Credential Manager de Windows (no en un archivo de texto).
/// </summary>
public static class CredencialAlmacen
{
    private const string Target = "SLCDM-DeviceToken";

    public static string? LeerToken()
    {
        if (!CredRead(Target, CredType.Generic, 0, out var credentialPtr))
        {
            return null;
        }

        try
        {
            var cred = Marshal.PtrToStructure<NativeCredential>(credentialPtr);
            return cred.CredentialBlob == IntPtr.Zero || cred.CredentialBlobSize == 0
                ? null
                : Marshal.PtrToStringUni(cred.CredentialBlob, (int)cred.CredentialBlobSize / 2);
        }
        finally
        {
            CredFree(credentialPtr);
        }
    }

    public static void GuardarToken(string token)
    {
        var blob = Encoding.Unicode.GetBytes(token);
        var native = new NativeCredential
        {
            Type = CredType.Generic,
            TargetName = Target,
            UserName = "device",
            CredentialBlob = Marshal.AllocHGlobal(blob.Length),
            CredentialBlobSize = (uint)blob.Length,
            Persist = CredPersist.LocalMachine
        };

        try
        {
            Marshal.Copy(blob, 0, native.CredentialBlob, blob.Length);
            if (!CredWrite(ref native, 0))
            {
                throw new InvalidOperationException(
                    $"No se pudo guardar el token en Credential Manager (Win32 {Marshal.GetLastWin32Error()}).");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(native.CredentialBlob);
        }
    }

    private enum CredType : uint
    {
        Generic = 1
    }

    private enum CredPersist : uint
    {
        LocalMachine = 2
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;
        public CredType Type;
        public string TargetName;
        public string? Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public CredPersist Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public string? TargetAlias;
        public string? UserName;
    }

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredRead(string target, CredType type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CredWrite([In] ref NativeCredential userCredential, [In] uint flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree([In] IntPtr buffer);
}
