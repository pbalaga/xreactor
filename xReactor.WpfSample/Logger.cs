#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.WpfSample
{
    public interface ILogger
    {
        void WriteLine(string str);
    }

    class InMemoryLogger : NotifyPropertyChangedBase, ILogger
    {
        StringBuilder builder = new StringBuilder();
        Lazy<string> cachedText;

        public string Text
        {
            get
            {
                return cachedText != null ?
                    cachedText.Value : null;
            }
        }

        public void WriteLine(string str)
        {
            builder.AppendLine(str);
            cachedText = new Lazy<string>(() => builder.ToString());
            RaisePropertyChanged(() => Text);
        }
    }
}
