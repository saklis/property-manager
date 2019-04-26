using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyManager
{
    internal class PropertyLine : PropertyEntry
    {
        /// <summary>
        /// Is this entry a comment without actual value?
        /// </summary>
        public bool IsCommentOrEmpty { get; set; }

        /// <summary>
        /// Source reference image. Stored to be able to overwrite it to source on save.
        /// </summary>
        public string Source { get; set; }
    }
}
