using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockupServer
{
    public class State
    {
        public static bool IsRecording = false;
        public static void StartRecord()
        {
            IsRecording = true;
        }
        public static void StopRecord()
        {
            IsRecording = false;
        }
        public static void ToggleRecord()
        {
            IsRecording = !IsRecording;
        }
    }
}
