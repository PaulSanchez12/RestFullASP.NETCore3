using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mappingDirectionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDirectionary)
        {
            _mappingDirectionary = mappingDirectionary ??
                throw new ArgumentNullException(nameof(mappingDirectionary));
        }
    }
}
