using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF
{
    public sealed class CognitiveHelper
    {
        public string Key { get; set; }
        public string UriBase { get; set; }

        private static readonly CognitiveHelper instance = new CognitiveHelper();

        private CognitiveHelper()
        {

        }
        
        public static CognitiveHelper Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
