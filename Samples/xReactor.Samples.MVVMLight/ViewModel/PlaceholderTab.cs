#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    public class PlaceholderTab : TabViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PlaceholderTab"/> class.
        /// </summary>
        public PlaceholderTab(MainViewModel mainViewModel)
            : base(mainViewModel)
        {
            //Autonumbering of placeholder tabs in format "Placeholder 1/4", etc.
            int currentCount = mainViewModel.Tabs.Count(tab => tab is PlaceholderTab) + 1;
            React.To(() => mainViewModel.Tabs.Count(tab => tab is PlaceholderTab))
                .Set(totalCount => this.Header = string.Format("Placeholder {0}/{1}", currentCount, totalCount));
        }
    }
}
