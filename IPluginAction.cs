using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    public interface IPluginAction
    {
        Task InitAsync(CancellationToken cancellationToken);
        Task UpdateAsync(CancellationToken cancellationToken = default);
        void DataUpdate();
    }
}
