#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace xReactor
{
    public class ItemWithPrevious<T>
    {
        public T Previous;
        public T Current;
    }

    public static class CustomRxExtensions
    {
        public static IObservable<ItemWithPrevious<T>> CombineWithPrevious<T>(this IObservable<T> source)
        {
            var previous = default(T);

            return source
                .Select(t => new ItemWithPrevious<T> { Previous = previous, Current = t })
                .Do(items => previous = items.Current);
        }
    }
}
