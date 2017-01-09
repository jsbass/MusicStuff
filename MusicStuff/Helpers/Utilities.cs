using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MidiParser.Helpers
{
    public static class Utilities
    {
        public static uint ConvertToUInt32(byte[] bytes, int startIndex = 0)
        {
            var array = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                if (i + startIndex < bytes.Length)
                {
                    array[i] = bytes[i + startIndex];
                }
                else
                {
                    array[i] = 0;
                }
            }

            return BitConverter.ToUInt32(array, 0);
        }
    }
}