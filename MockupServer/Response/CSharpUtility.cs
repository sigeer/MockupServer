using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Newtonsoft.Json;
using System.Reflection;

namespace MockupServer.Response
{
    public class CSharpUtility
    {
        static Random random = new Random();
        static HashSet<Type> numericTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
            typeof(decimal)
        };

        public static object GenerateObjectByCSharpClass(string inputClassString)
        {
            string code = $@"
        using System;
        using System.Collections.Generic;

        namespace DynamicNamespace
        {{
          {inputClassString}  
        }}";

            // 编译代码
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "DynamicAssembly",
                new[] { syntaxTree },
                new[]
                {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location)
                },
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
                        var dynamicClassType = assembly.GetTypes();
                        var rootTypes = dynamicClassType.Where(x => x.Name == "Root");
                        if (rootTypes.Count() == 0)
                            throw new Exception("没找到Root类");

                        if (rootTypes.Count() > 1)
                            throw new Exception("重复声明了Root类");

                        var entryProperty = rootTypes.First()!.GetProperty("data");
                        if (entryProperty == null)
                            throw new Exception("Root类中没找到data属性");

                        return GenerateDataByType(entryProperty.PropertyType);
                    }
                    catch (Exception ex)
                    {
                        return new { text = "error", message = ex.Message };
                    }
                }
            }
        }

        public static object GenerateDataByType(Type propertyType)
        {
            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var realType = nullableType ?? propertyType;
            if (propertyType == typeof(string))
            {
                return GenerateRandomString(10);
            }
            else if (realType == typeof(bool))
            {
                return DateTime.Now.Ticks % 2 == 0;
            }
            else if (realType == typeof(char))
            {
                return GenerateRandomChar();
            }
            else if (numericTypes.Contains(realType))
            {
                return GenerateRandomNumber(realType);
            }
            else if (propertyType.IsGenericType)
            {
                var genericArguments = propertyType.GetGenericArguments();
                if (propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elementType = genericArguments[0]!;
                    object listInstance = Activator.CreateInstance(propertyType)!;
                    var addMethod = propertyType.GetMethod("Add")!;
                    var listLength = random.Next(10);
                    for (int i = 0; i < listLength; i++)
                    {
                        addMethod.Invoke(listInstance, new object[] { GenerateDataByType(elementType) });
                    }
                    return listInstance;
                }
                if (propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = genericArguments[0];
                    var valueType = Nullable.GetUnderlyingType(genericArguments[1]) ?? genericArguments[1];
                    object listInstance = Activator.CreateInstance(propertyType)!;
                    var addMethod = propertyType.GetMethod("Add")!;
                    var listLength = random.Next(10);
                    for (int i = 0; i < listLength; i++)
                    {
                        addMethod.Invoke(listInstance, new object[] { GenerateDataByType(keyType), GenerateDataByType(valueType) });
                    }
                    return listInstance;
                }

            }
            else if (propertyType.IsArray)
            {
                var elementType = propertyType.GetElementType()!;
                Array arrayInstance = Array.CreateInstance(elementType, random.Next(10));
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

        private static object GenerateRandomObject(Type responseType)
        {
            object obj = Activator.CreateInstance(responseType);

            // 获取所有公共属性
            PropertyInfo[] properties = responseType.GetProperties();

            // 为每个属性赋予随机字符的值
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(obj, GenerateDataByType(property.PropertyType));
            }
            return obj;
        }

        private static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static char GenerateRandomChar()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return chars[random.Next(chars.Length)];
        }

        private static object GenerateRandomNumber(Type type)
        {
            if (type == typeof(int))
                return random.Next();
            else if (type == typeof(double))
                return random.NextDouble();
            else if (type == typeof(decimal))
                return (decimal)random.NextDouble() + random.Next();
            else if (type == typeof(float))
                return random.NextSingle();
            else if (type == typeof(long))
                return random.NextInt64();
            else if (type == typeof(short))
                return random.Next(short.MaxValue);
            else if (type == typeof(ushort))
                return random.Next(ushort.MaxValue);
            else if (type == typeof(byte))
                return random.Next(byte.MaxValue);
            else if (type == typeof(sbyte))
                return random.Next(sbyte.MaxValue);
            else
            {
                return 0;
            }
        }
    }
}
