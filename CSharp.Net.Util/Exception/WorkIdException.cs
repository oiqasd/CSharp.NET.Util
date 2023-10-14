using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Net.Util
{
    internal class WorkIdException:BaseException
    {
        public WorkIdException(string message) : base(message) { }
    }
}
