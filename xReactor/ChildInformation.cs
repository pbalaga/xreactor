#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    /// <summary>
    /// Provides info on the child of a certain expression
    /// (the sub-expression).
    /// </summary>
    struct ChildInformation
    {
        public static readonly ChildInformation Null = new ChildInformation((UsedSubproperty)null);
        public static readonly ChildInformation MethodBreak = new ChildInformation(MemberTypes.Method);
        public static readonly ChildInformation FieldBreak = new ChildInformation(MemberTypes.Field);

        const MemberTypes MemberTypeUnknown = (MemberTypes)(-1);

        public UsedSubproperty Property;
        public MemberTypes MemberType;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ChildInformation"/> class.
        /// </summary>
        public ChildInformation(UsedSubproperty subProperty)
        {
            this.Property = subProperty;
            this.MemberType = subProperty == null ? MemberTypeUnknown : MemberTypes.Property;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ChildInformation"/> class.
        /// </summary>
        public ChildInformation(MemberTypes childMemberType)
        {
            this.MemberType = childMemberType;
            this.Property = null;
        }

        public bool IsTrackable
        {
            get
            {
                return MemberType == MemberTypes.Property
                    || MemberType == MemberTypes.Method;
            }
        }
    }
}
