#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public static class FlattenExtension
    {
        public static IEnumerable<T> FlattenSelect<T>(this IEnumerable<T> collection,
            Func<T, T> selectorFunc)
        {
            var selected = collection.Select(selectorFunc);
            foreach (var item in selected)
            {
                yield return item;
            }

            if (selected.Any())
                foreach (var inner in FlattenSelect(selected, selectorFunc))
                {
                    yield return inner;
                }
        }
    }
}
