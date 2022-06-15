using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Prinubes.Common.Helpers
{
    public class PropertyCopier
    {
        public static void CopyStringTypes<TParent, TChild>(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties.Where(x => x.PropertyType == typeof(string)))
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        if (!string.IsNullOrWhiteSpace(parentProperty.GetValue(parent)?.ToString()))
                        {
                            childProperty.SetValue(child, parentProperty.GetValue(parent));
                            break;
                        }
                    }
                }
            }
        }
        public static void Populate(object from, object to, string[]? excludeAttributes = null)
        {
            if (from is null)
            {
                throw new ArgumentNullException(nameof(from));
            }
            if (excludeAttributes is null)
            {
                excludeAttributes = new string[0] { };
            }
            var sourceType = from.GetType();
            foreach (PropertyInfo target in to.GetType().GetProperties())
            {
                if (excludeAttributes.Any(x => string.Equals(x,target.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }
                var isUpdatable = target.CanWrite && (target.GetCustomAttribute<NotMappedAttribute>(true) == null);
                if (isUpdatable)
                {
                    var source = sourceType.GetProperty(target.Name);
                    if (source != null)
                    {
                        var @default = sourceType.IsValueType ? Activator.CreateInstance(sourceType) : null;
                        var equality = (IEqualityComparer)typeof(EqualityComparer<>).MakeGenericType(sourceType).GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                        var value = source.GetValue(from);
                        if (!equality.Equals(value, @default))
                        {
                            target.SetValue(to, value, null);
                        }
                    }
                }
            }
        }
    }
}
