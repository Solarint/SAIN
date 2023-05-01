using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryOrganizingFeatures.Reflections
{
    internal class Grid : ReflectionBase
    {
        public Grid(object instance)
        {
            ReflectedInstance = instance;
            ReflectedType = instance.GetType();
        }

        public IEnumerable<Item> Items
        {
            get
            {
                return GetPropertyValue<IEnumerable<Item>>("Items");
            }
        }

        public object FindLocationForItem(Item item)
        {
            return InvokeMethod("FindLocationForItem", new object[] { item });
        }
    }
}
