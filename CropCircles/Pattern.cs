using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CropCircles
{
    class Pattern
    {
        public String name;
        public int sizeX;
        public int sizeY;
        public int[][] pattern;

        public Pattern(String name, int x, int y, int[][] pattern)
        {
            this.name = name;
            this.sizeX = x;
            this.sizeY = y;
            this.pattern = pattern;
        }
    }
}
