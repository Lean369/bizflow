using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Business
{
    public class MoreTimeEventArgs : EventArgs
    {
        private MoreTimeResult result;

        public MoreTimeResult Result
        {
            get
            {
                return this.result;
            }
        }

        public MoreTimeEventArgs(MoreTimeResult result)
        {
            this.result = result;
        }
    }
}
