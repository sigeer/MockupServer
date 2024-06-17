using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MockupServer.Response
{
    public class CSharpUtility
    {
        Random random = new Random();
        static HashSet<Type> numericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(decimal)
        };

        Type[] allTypes = new Type[0];

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

            // 编译代码
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                  .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                  .Select(a => MetadataReference.CreateFromFile(a.Location))
                  .Cast<MetadataReference>()
                  .Concat(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(RangeAttribute).GetTypeInfo().Assembly.Location)
                });

            var compilation = CSharpCompilation.Create(
                "DynamicAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    // 编译失败，输出错误信息
                    foreach (Diagnostic diagnostic in result.Diagnostics)
                    {
                        Console.Error.WriteLine(diagnostic.ToString());
                    }
                    return new { text = "error", message = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString())) };
                }
                else
                {
                    try
                    {
                        // 编译成功，加载程序集
                        ms.Seek(0, SeekOrigin.Begin);
                        Assembly assembly = Assembly.Load(ms.ToArray());

                        // 使用反射获取类
                        allTypes = assembly.GetTypes();
                        var rootTypes = allTypes.Where(x => x.Name == "Root");
                        if (rootTypes.Count() == 0)
                            throw new Exception("没找到Root类");

                        if (rootTypes.Count() > 1)
                            throw new Exception("重复声明了Root类");

                        var entryProperty = rootTypes.First()!.GetProperty("data");
                        if (entryProperty == null)
                            throw new Exception("Root类中没找到data属性");

                        return GenerateDataByProperty(entryProperty);
                    }
                    catch (Exception ex)
                    {
                        return new { text = "error", message = ex.Message };
                    }
                }
            }
        }

        public object? GenerateDataByType(Type propertyType, PropertyInfo? propertyInfo = null)
        {
            var rangeAttr = propertyInfo?.GetCustomAttribute<RangeAttribute>();
            int min = (int?)rangeAttr?.Minimum ?? 0; int max = (int?)rangeAttr?.Maximum ?? random.Next(10);

            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var realType = nullableType ?? propertyType;

            if (nullableType != null && random.NextDouble() < 0.2)
                return null;

            if (propertyType == typeof(string))
            {
                if (random.NextDouble() < 0.2)
                    return null;

                return GenerateRandomString(max);
            }
            else if (realType == typeof(bool))
            {
                return DateTime.Now.Ticks % 2 == 0;
            }
            else if (realType == typeof(char))
            {
                return GenerateRandomChar();
            }
            else if (realType == typeof(DateTime))
            {
                return DateTime.Now;
            }
            else if (numericTypes.Contains(realType))
            {
                return GenerateRandomNumber(realType, min, max);
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
                    var listLength = random.Next(min, max );
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
                    var listLength = random.Next(min, max);
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
                Array arrayInstance = Array.CreateInstance(elementType, random.Next(min, max));
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

            // 获取所有公共属性
            PropertyInfo[] properties = responseType.GetProperties();

            // 为每个属性赋予随机字符的值
            foreach (PropertyInfo property in properties)
            {
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
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private char GenerateRandomChar()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return chars[random.Next(chars.Length)];
        }
        private object GenerateRandomNumber(Type type, int min, int? max = null)
        {
            if (type == typeof(int))
                return random.Next(min, max ?? int.MaxValue);
            else if (type == typeof(double))
                return random.NextDouble() * random.Next(min, max ?? int.MaxValue);
            else if (type == typeof(decimal))
                return (decimal)random.NextDouble() + random.Next(min, max ?? int.MaxValue);
            else if (type == typeof(float))
                return random.NextSingle() * random.Next(min, max ?? int.MaxValue);
            else if (type == typeof(long))
                return random.NextInt64(min, max ?? long.MaxValue);
            else if (type == typeof(short))
                return random.Next(min, max ?? short.MaxValue);
            else if (type == typeof(ushort))
                return random.Next(min, max ?? ushort.MaxValue);
            else if (type == typeof(byte))
                return random.Next(min, max ?? byte.MaxValue);
            else if (type == typeof(sbyte))
                return random.Next(min, max ?? sbyte.MaxValue);
            else
            {
                return 0;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultAttribute:Attribute
    {
        public object value;

        public DefaultAttribute(object value)
        {
            this.value = value;
        }
    }
}
