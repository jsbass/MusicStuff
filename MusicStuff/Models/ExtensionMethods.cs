using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using MusicStuff.Models.Midi;

namespace MusicStuff.Models
{
    public static class Fractions
    {
        public static int GetGcd(int a, int b)
        {
            if (b == 0) return a;

            return GetGcd(b, a%b);
        }

        public static int GetLcm(int a, int b)
        {
            return a*b/GetGcd(a, b);
        }
    }
}