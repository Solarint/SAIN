using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.BotSettings.Categories
{
    public class FieldsCache
    {
        public FieldsCache(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public)
        {
            Type = type;
            Flags = flags;
        }

        readonly BindingFlags Flags;
        readonly Type Type;

        public FieldInfo[] GetFields(Type type)
        {
            return Fields[type];
        }

        public void CacheHandler(bool cacheFields)
        {
            bool cacheSaved = Fields.Count > 0;
            if (cacheFields && !cacheSaved)
            {
                foreach (FieldInfo field in Type.GetFields(Flags))
                {
                    Type type = field.FieldType;
                    Fields.Add(type, type.GetFields(Flags));
                }
            }
            else if (!cacheFields && cacheSaved)
            {
                Fields.Clear();
            }
        }

        public readonly Dictionary<Type, FieldInfo[]> Fields = new Dictionary<Type, FieldInfo[]>();
    }
}
