/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema: GenesisGest.Net / Nexum Altivon
 * Versão: 1.1.5
 */

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace NexumAltivon.Desktop.Services;

public sealed record SavedDesktopCredential(string Email, string Password);

public static class DesktopCredentialStore
{
    private const string TargetPrefix = "GenesisGest.Net/NexumAltivon/Login/";
    private const uint GenericCredential = 1;
    private const uint PersistLocalMachine = 2;
    private const int NotFound = 1168;

    public static SavedDesktopCredential? Load(string scope)
    {
        if (!CredRead(Target(scope), GenericCredential, 0, out var pointer))
        {
            var error = Marshal.GetLastWin32Error();
            if (error == NotFound) return null;
            throw new Win32Exception(error, "O Gerenciador de Credenciais do Windows não retornou a credencial desta tela.");
        }

        try
        {
            var item = Marshal.PtrToStructure<Credential>(pointer);
            var email = Marshal.PtrToStringUni(item.UserName) ?? string.Empty;
            var password = item.CredentialBlob == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(item.CredentialBlob) ?? string.Empty;
            return string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password) ? null : new SavedDesktopCredential(email, password);
        }
        finally { CredFree(pointer); }
    }

    public static void Save(string scope, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("E-mail obrigatório.", nameof(email));
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Senha obrigatória.", nameof(password));
        var bytes = Encoding.Unicode.GetByteCount(password);
        if (bytes > 5120) throw new ArgumentOutOfRangeException(nameof(password), "A senha excede o limite do Gerenciador de Credenciais do Windows.");

        var target = Marshal.StringToHGlobalUni(Target(scope));
        var user = Marshal.StringToHGlobalUni(email.Trim());
        var secret = Marshal.StringToHGlobalUni(password);
        try
        {
            var credential = new Credential
            {
                Type = GenericCredential,
                TargetName = target,
                CredentialBlobSize = (uint)bytes,
                CredentialBlob = secret,
                Persist = PersistLocalMachine,
                UserName = user
            };
            if (!CredWrite(ref credential, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "O Gerenciador de Credenciais do Windows não conseguiu salvar a credencial.");
        }
        finally
        {
            Marshal.FreeHGlobal(target);
            Marshal.FreeHGlobal(user);
            Marshal.ZeroFreeGlobalAllocUnicode(secret);
        }
    }

    public static void Delete(string scope)
    {
        if (CredDelete(Target(scope), GenericCredential, 0)) return;
        var error = Marshal.GetLastWin32Error();
        if (error != NotFound) throw new Win32Exception(error, "O Gerenciador de Credenciais do Windows não conseguiu remover a credencial.");
    }

    private static string Target(string scope)
    {
        if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Escopo da tela de login obrigatório.", nameof(scope));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scope.Trim().ToLowerInvariant())));
        return TargetPrefix + hash;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Credential
    {
        public uint Flags;
        public uint Type;
        public IntPtr TargetName;
        public IntPtr Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public IntPtr TargetAlias;
        public IntPtr UserName;
    }

    [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);
    [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredWrite(ref Credential credential, uint flags);
    [DllImport("Advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CredDelete(string target, uint type, uint flags);
    [DllImport("Advapi32.dll", EntryPoint = "CredFree")] private static extern void CredFree(IntPtr buffer);
}
