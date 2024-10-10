using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CredentialManagerPOC.Services
{
    public interface ICredentialService
    {
        string GetConnectionString(string targetName);
    }

    public class CredentialService : ICredentialService
    {
        public string GetConnectionString(string targetName)
        {
            try
            {
                var cred = ReadCredential(targetName);
                return cred?.Password; // Retorna a senha armazenada que representa a connection string
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar credencial: {ex.Message}");
                throw; // Relança a exceção para tratamento
            }
        }

        private Credential ReadCredential(string targetName)
        {
            IntPtr credPtr;
            bool read = CredRead(targetName, CRED_TYPE.GENERIC, 0, out credPtr);
            if (!read)
            {
                return null;
            }

            using (CriticalCredentialHandle handle = new CriticalCredentialHandle(credPtr))
            {
                var credential = handle.GetCredential();
                return credential;
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);

        private enum CRED_TYPE : int
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,
            MAXIMUM_EX = (MAXIMUM + 1000),
        }

        private class Credential
        {
            public string UserName { get; private set; }
            public string Password { get; private set; }

            public Credential(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }

        private class CriticalCredentialHandle : Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
        {
            public CriticalCredentialHandle(IntPtr preexistingHandle) : base(true)
            {
                SetHandle(preexistingHandle);
            }

            protected override bool ReleaseHandle()
            {
                if (!IsInvalid)
                {
                    CredFree(handle); // Chamada correta para liberar memória
                    SetHandleAsInvalid();
                }
                return true;
            }

            public Credential GetCredential()
            {
                var ncred = (NativeCredential)Marshal.PtrToStructure(handle, typeof(NativeCredential));
                string userName = Marshal.PtrToStringUni(ncred.UserName);
                string password = Marshal.PtrToStringUni(ncred.CredentialBlob, (int)ncred.CredentialBlobSize / 2);
                return new Credential(userName, password);
            }
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr buffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NativeCredential
        {
            public int Flags;
            public int Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public long LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }
    }
}
