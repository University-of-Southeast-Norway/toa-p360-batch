#if NET48
using System.Threading.Tasks;
using System.IO;
using System;
#endif

namespace DfoToa.Domain
{
    public interface IReport
    {
        Task Report(string content);
    }
}
