using System;
using System.Collections.Generic;
using System.Text;

namespace autoMakeromCLI
{
    public class DecKeyModel
    {
        public string TitleId { get; set; }
        public string DecKey { get; set; }

        public DecKeyModel(string titleId, string decKey)
        {
            TitleId = titleId;
            DecKey = decKey;
        }

        public DecKeyModel()
        {

        }
    }
}
