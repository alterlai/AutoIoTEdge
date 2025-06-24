using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace AutoIoTEdge.Models;

public abstract class ModelTwinBase : IModuleTwin
{
        /// <summary>
        /// Populates the twin properties from a TwinCollection using reflection.
        /// </summary>
        public void UpdateFromTwin(TwinCollection collection)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (collection.Contains(prop.Name) && prop.CanWrite)
                {
                    var value = collection[prop.Name];
                    if (value != null)
                    {
                        prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }
        }

        /// <summary>
        /// Populates the twin properties from a configuration section (e.g., appsettings.json).
        /// </summary>
        public void UpdateFromConfiguration(IConfiguration config)
        {
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = config[prop.Name];
                if (value != null && prop.CanWrite)
                {
                    prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType));
                }
            }
        }

        /// <summary>
        /// Converts the twin properties to a TwinCollection.
        /// </summary>
        public TwinCollection ToTwinCollection()
        {
            var collection = new TwinCollection();
            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                collection[prop.Name] = prop.GetValue(this);
            }
            return collection;
        }
}