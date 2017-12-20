#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace xReactor.Samples.MVVMLight
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:App"/> class.
        /// </summary>
        public App()
        {
            //System.Reactive.Linq.Observable.Return(1).Subscribe(_ => { throw new Exception(); });
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.FailFast;
        }
    }
}
