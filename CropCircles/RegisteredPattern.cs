using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CropCircles
{
    class RegisteredPattern
    {
        public Pattern patt;
        public int x;
        public int y;

        public RegisteredPattern(Pattern p, int x, int y)
        {
            this.patt = p;
            this.x = x;
            this.y = y;
        }
    }
}
