using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Shared.UI.Framework.Abstractions
{
    internal interface IRaiseCanExecuteChanged
    {
        event EventHandler CanExecuteChanged;
        void RaiseCanExecuteChanged();
    }
}
