#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Common
{
    public class Product : CommonBase
    {
        public Product(ProductInfo productInfo)
        {
            if (productInfo == null)
                throw new ArgumentNullException("productInfo");

            this.ProductInfo = productInfo;
            isAvailableProperty = this.Create(() => IsAvailable, () => ProductInfo.IsAvailable);
        }

        /// <summary>
        /// This must be a property, if we want to build reactive
        /// expressions upon it.
        /// Fields are not considered safe, because there is no way
        /// to track field read/write operations. Therefore,
        /// xReactor might attach some handler and be unable
        /// to dettach them, in result keeping a reference
        /// to an unused instance.
        /// That being said, there is no guarantee that 
        /// properties will publish property change notifications 
        /// (as in case of <see cref="ProductInfo"/>).
        /// </summary>
        public ProductInfo ProductInfo
        {
            get;
            private set;
        }   

        /// <summary>
        /// Values of this property are forwarded from the 
        /// 'productInfo' instance, which is a raw INPC
        /// object, not aware of reactive patterns.
        /// </summary>
        private Property<bool> isAvailableProperty;
        public bool IsAvailable
        {
            get { return isAvailableProperty.Value; }
            set { isAvailableProperty.Value = value; }
        }
    }
}
