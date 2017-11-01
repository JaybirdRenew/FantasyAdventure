using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TextAdventure
{
    ///<summary>
    /// Midi requirements from http://www.codeguru.com/columns/dotnet/making-music-with-midi-and-c.html
    /// Author: Peter Shaw
    /// Date: June 17th, 2015
    ///</summary> 
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiOutCaps
    {
        public UInt16 wMid;
        public UInt16 wPid;
        public UInt32 vDriverVersion;

        [MarshalAs(UnmanagedType.ByValTStr,
           SizeConst = 32)]
        public String szPname;

        public UInt16 wTechnology;
        public UInt16 wVoices;
        public UInt16 wNotes;
        public UInt16 wChannelMask;
        public UInt32 dwSupport;

        internal static string Mci(string command)
        {
            int returnLength = 256;
            StringBuilder reply = new StringBuilder(returnLength);
            mciSendString(command, reply, returnLength, IntPtr.Zero);
            return reply.ToString();
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);
    }
}
