using System;
using System.Collections.Generic;
namespace BluePrinceArchipelago.Core
{
    public class TrapManager
    {
        public static List<string> validTypes = ["freeze", "step", "eod", "star"];
    }
    public class Trap(string trapType, int trapAmount = 1) {
        private string _TrapType = trapType;
        public string TrapType 
        { 
            get { return _TrapType; }
            set { if (TrapManager.validTypes.Contains(value)) _TrapType = value; }
        }

        private int _TrapAmount = trapAmount;
        public int TrapAmount 
        { 
            get { return _TrapAmount; } 
            set { _TrapAmount = value; } 
        }
    }
}
