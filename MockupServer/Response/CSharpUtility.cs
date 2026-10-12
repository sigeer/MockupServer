using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MockupServer.Response
{
    public class CSharpUtility
    {
        static readonly Random SharedRandom = Random.Shared;
        static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(decimal)
        };

        static readonly char[] CharArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        static readonly object ReferenceLock = new();
        static MetadataReference[]? _cachedReferences;
        static readonly ConcurrentDictionary<string, Assembly> CompileCache = new();

        readonly Microsoft.Extensions.Logging.ILogger? _logger;
        Type[] allTypes = Array.Empty<Type>();

        public CSharpUtility(Microsoft.Extensions.Logging.ILogger? logger = null)
        {
            _logger = logger;
        }

        static MetadataReference[] GetMetadataReferences()
        {
            if (_cachedReferences != null)
                return _cachedReferences;

            lock (ReferenceLock)
            {
                if (_cachedReferences != null)
                    return _cachedReferences;

                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Cast<MetadataReference>()
                    .Concat(new[]
                    {
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(RangeAttribute).GetTypeInfo().Assembly.Location)
                    })
                    .ToArray();

                _cachedReferences = references;
                return references;
            }
        }

        public object GenerateObjectByCSharpClass(string inputClassString)
        {
            string code = $@"
        using System;
        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;
        using MockupServer.Response;

        namespace DynamicNamespace
        {{
          {inputClassString}
        }}";

            Assembly assembly;
            try
            {
                assembly = CompileCache.GetOrAdd(inputClassString, _ => Compile(inputClassString, code));
            }
            catch (Exception ex)
            {
                // 编译失败保持返回错误对象的既有行为
                return new { text = "error", message = ex.Message };
            }

            allTypes = assembly.GetTypes();
            var rootTypes = allTypes.Where(x => x.Name == "Root").ToArray();
            if (rootTypes.Length == 0)
                throw new Exception("没找到Root类");

            if (rootTypes.Length > 1)
                throw new Exception("重复声明了Root类");

            var entryProperty = rootTypes[0].GetProperty("data");
            if (entryProperty == null)
                throw new Exception("Root类中没找到data属性");

            try
            {
                return GenerateDataByProperty(entryProperty)!;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "生成随机数据失败");
                return new { text = "error", message = ex.Message };
            }
        }

        static Assembly Compile(string inputClassString, string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "DynamicAssembly",
                new[] { syntaxTree },
                GetMetadataReferences(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                var message = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
                Log.Logger.Error("随机数据C#代码编译失败：{Diagnostics}", message);
                throw new Exception(message);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }

        public object? GenerateDataByType(Type propertyType, PropertyInfo? propertyInfo = null)
        {
            var rangeAttr = propertyInfo?.GetCustomAttribute<RangeAttribute>();
            int min = rangeAttr != null ? Convert.ToInt32(rangeAttr.Minimum) : 0;
            int lengthMax = rangeAttr != null ? Convert.ToInt32(rangeAttr.Maximum) : SharedRandom.Next(10);

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var realType = nullableType ?? propertyType;

            if (nullableType != null && SharedRandom.NextDouble() < 0.2)
                return null;

            if (propertyType == typeof(string))
            {
                if (SharedRandom.NextDouble() < 0.2)
                    return null;

                return GenerateRandomString(lengthMax);
            }
            else if (realType == typeof(bool))
            {
                return SharedRandom.Next(2) == 0;
            }
            else if (realType == typeof(char))
            {
                return GenerateRandomChar();
            }
            else if (realType == typeof(DateTime))
            {
                return DateTime.Now;
            }
            else if (NumericTypes.Contains(realType))
            {
                return GenerateRandomNumber(realType, min, rangeAttr != null ? Convert.ToInt32(rangeAttr.Maximum) : null);
            }
            else if (propertyType.IsGenericType)
            {
                var genericType = propertyType.GetGenericTypeDefinition();
                var genericArguments = propertyType.GetGenericArguments();
                if (genericType == typeof(List<>))
                {
                    var elementType = genericArguments[0]!;
                    object listInstance = Activator.CreateInstance(propertyType)!;
                    var addMethod = propertyType.GetMethod("Add")!;
                    var listLength = SharedRandom.Next(min, lengthMax);
                    for (int i = 0; i < listLength; i++)
                    {
                        addMethod.Invoke(listInstance, new object?[] { GenerateDataByType(elementType) });
                    }
                    return listInstance;
                }
                if (genericType == typeof(Dictionary<,>))
                {
                    var keyType = genericArguments[0];
                    var valueType = Nullable.GetUnderlyingType(genericArguments[1]) ?? genericArguments[1];
                    object dictionary = Activator.CreateInstance(propertyType)!;
                    var addMethod = propertyType.GetMethod("Add")!;
                    var listLength = SharedRandom.Next(min, lengthMax);
                    for (int i = 0; i < listLength; i++)
                    {
                        addMethod.Invoke(dictionary, new object?[] { GenerateDataByType(keyType)!, GenerateDataByType(valueType) });
                    }
                    return dictionary;
                }
                if (allTypes.Contains(genericType))
                {
                    return GenerateRandomObject(propertyType);
                }
            }
            else if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType()!;
                Array arrayInstance = Array.CreateInstance(elementType, SharedRandom.Next(min, lengthMax));
                for (int i = 0; i < arrayInstance.Length; i++)
                {
                    arrayInstance.SetValue(GenerateDataByType(elementType), i);
                }
                return arrayInstance;
            }
            else if (propertyType.IsClass)
            {
                return GenerateRandomObject(propertyType);
            }
            throw new Exception($"暂不支持该类型的随机生成{propertyType.FullName}");
        }

        public object? GenerateDataByProperty(PropertyInfo propery)
        {
            return GenerateDataByType(propery.PropertyType, propery);
        }

        private object GenerateRandomObject(Type responseType)
        {
            object obj = Activator.CreateInstance(responseType)!;

            foreach (PropertyInfo property in responseType.GetProperties())
            {
                if (!property.CanWrite)
                    continue;

                var defaultAttr = property.GetCustomAttribute<DefaultAttribute>();
                if (defaultAttr != null)
                    property.SetValue(obj, defaultAttr.value);
                else
                    property.SetValue(obj, GenerateDataByType(property.PropertyType, property));
            }
            return obj;
        }

        private string GenerateRandomString(int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = CharArray[SharedRandom.Next(CharArray.Length)];
            }
            return new string(chars);
        }

        private char GenerateRandomChar()
        {
            return CharArray[SharedRandom.Next(CharArray.Length)];
        }

        private object GenerateRandomNumber(Type type, int min, int? max = null)
        {
            if (type == typeof(int))
                return SharedRandom.Next(min, max ?? int.MaxValue);
            else if (type == typeof(double))
                return SharedRandom.NextDouble() * SharedRandom.Next(min, max ?? int.MaxValue);
            else if (type == typeof(decimal))
                return (decimal)SharedRandom.NextDouble() + SharedRandom.Next(min, max ?? int.MaxValue);
            else if (type == typeof(float))
                return SharedRandom.NextSingle() * SharedRandom.Next(min, max ?? int.MaxValue);
            else if (type == typeof(long))
                return SharedRandom.NextInt64(min, max ?? long.MaxValue);
            else if (type == typeof(short))
                return (short)SharedRandom.Next(min, max ?? short.MaxValue);
            else if (type == typeof(ushort))
                return (ushort)SharedRandom.Next(min, max ?? ushort.MaxValue);
            else if (type == typeof(byte))
                return (byte)SharedRandom.Next(min, max ?? byte.MaxValue);
            else if (type == typeof(sbyte))
                return (sbyte)SharedRandom.Next(min, max ?? sbyte.MaxValue);
            else if (type == typeof(uint))
                return (uint)SharedRandom.NextInt64(min, max ?? (long)uint.MaxValue);
            else if (type == typeof(ulong))
                return (ulong)SharedRandom.NextInt64(min, max ?? long.MaxValue);
            else
            {
                return 0;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultAttribute : Attribute
    {
        public object value;

        public DefaultAttribute(object value)
        {
            this.value = value;
        }
    }
}
