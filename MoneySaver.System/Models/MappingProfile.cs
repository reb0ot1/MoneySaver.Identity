using AutoMapper;
using System.Reflection;

namespace MoneySaver.System.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile(Assembly assembly)
        {
            this.ApplyMappingFromAssembly(assembly);
        }

        private void ApplyMappingFromAssembly(Assembly assembly)
        {
            var types = assembly
                .GetExportedTypes()
                .Where(t => t
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                const string mappingMethodName = "Mapping";
                var methodInfo = type.GetMethod(mappingMethodName)
                    ?? type.GetInterface("IMapFrom`1")?.GetMethod(mappingMethodName);

                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}
