using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AutoIoTEdge.Models;

public abstract class ModuleTwinBase : IModuleTwin
{
        /// <summary>
        /// Populates the twin properties from a TwinCollection using reflection.
        /// </summary>
        public void UpdateFromTwin(TwinCollection collection)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (collection.Contains(prop.Name) && prop.CanWrite)
                {
                    var value = collection[prop.Name];
                    if (value != null)
                    {
                        // Check if the property is static by examining its getter method
                        var isStatic = prop.GetMethod?.IsStatic == true;
                        var instance = isStatic ? null : this;
                        prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }
        }

        /// <summary>
        /// Populates the twin properties from a configuration section (e.g., appsettings.json).
        /// </summary>
        public void UpdateFromConfiguration(IConfiguration config)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var value = config[prop.Name];
                if (value != null && prop.CanWrite)
                {
                    // Check if the property is static by examining its getter method
                    var isStatic = prop.GetMethod?.IsStatic == true;
                    var instance = isStatic ? null : this;
                    prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType));
                }
            }
        }

        /// <summary>
        /// Converts the twin properties to a TwinCollection.
        /// </summary>
        public TwinCollection ToTwinCollection()
        {
            var collection = new TwinCollection();
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                // Check if the property is static by examining its getter method
                var isStatic = prop.GetMethod?.IsStatic == true;
                var instance = isStatic ? null : this;
                collection[prop.Name] = prop.GetValue(instance);
            }
            return collection;
        }
}