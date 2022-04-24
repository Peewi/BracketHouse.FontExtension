using System.Collections.Generic;
using System.Linq;

namespace BracketHouse.FontExtension
{
    public class FontDescription
    {
        public FontDescription(string path, params char[] characters)
        {
            this.Path = path;            
            this.Characters = characters;
        }

        public string Path { get; }        
        public char[] Characters { get; }
    }
}
