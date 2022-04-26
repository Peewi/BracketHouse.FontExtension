using System.Collections.Generic;
using System.Linq;

namespace BracketHouse.FontExtension
{
    internal class FontDescription
    {
        internal FontDescription(string path, params char[] characters)
        {
            this.Path = path;            
            this.Characters = characters;
        }

        internal string Path { get; }
        internal char[] Characters { get; }
    }
}
