#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{
    public static class Utility
    {
        public static IEnumerable<string> SelectNames<T>(this IEnumerable<UsedPropertyChain> properties)
        {
            return properties.Select(p => p.Name);
        }

        public static IEnumerable<INotifyPropertyChanged> SelectTargets<T>(this IEnumerable<UsedPropertyChain> properties)
        {
            return properties.Select(p => p.Parent);
        }

        /// <summary>
        /// Expands this top-level property collection by adding the nested properties.
        /// </summary>
        public static IEnumerable<UsedPropertyBase> IncludeNested(this IEnumerable<UsedPropertyChain> topLevelChainCollection)
        {
            foreach (var topLevelProperty in topLevelChainCollection)
            {
                yield return topLevelProperty;
            }

            foreach (var subProp in topLevelChainCollection.FlattenSelect<UsedPropertyBase>(c => c.Child))
            {
                yield return subProp;
            }
        }
    }
}
