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
                        var rootType = dynamicClassType.FirstOrDefault(x => x.Name == "Root");
                        if (rootType == null)
                            throw new Exception("至少要有一个Root类");
                        return GenerateRandomObject(rootType);
                    }
                    catch (Exception ex)
                    {
                        return new { text = "error", message = ex.Message };
                    }
                }
            }
        }

        public static object GenerateRandomObject(Type responseType)
        {
            object obj = Activator.CreateInstance(responseType);

            // 获取所有公共属性
            PropertyInfo[] properties = responseType.GetProperties();

            // 为每个属性赋予随机字符的值
            foreach (PropertyInfo property in properties)
            {
                var nullableType = Nullable.GetUnderlyingType(property.PropertyType);
                var realType = nullableType ?? property.PropertyType;
                if (property.PropertyType == typeof(string))
                {
                    // 生成随机字符串并赋值给属性
                    string randomValue = GenerateRandomString(10); // 生成长度为 10 的随机字符串
                    property.SetValue(obj, randomValue);
                }
                else if (realType == typeof(bool))
                {
                    property.SetValue(obj, DateTime.Now.Ticks % 2 == 0);
                }
                else if (realType == typeof(char))
                {
                    property.SetValue(obj, GenerateRandomChar());
                }
                else if (numericTypes.Contains(realType))
                {
                    property.SetValue(obj, GenerateRandomNumber(realType));
                }
                else if (property.PropertyType.IsGenericType)
                {
                    var genericArguments = property.PropertyType.GetGenericArguments();
                    if (property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var elementType = genericArguments[0]!;
                        object listInstance = Activator.CreateInstance(property.PropertyType)!;
                        var addMethod = property.PropertyType.GetMethod("Add")!;
                        var listLength = random.Next(10);
                        for (int i = 0; i < listLength; i++)
                        {
                            addMethod.Invoke(listInstance, new object[] { GenerateRandomObject(elementType) });
                        }
                        property.SetValue(obj, listInstance);
                    }

                }
                else if (property.PropertyType.IsArray)
                {
                    var elementType = property.PropertyType.GetElementType();
                    Array arrayInstance = Array.CreateInstance(elementType, random.Next(10));
                    for (int i = 0; i < arrayInstance.Length; i++)
                    {
                        arrayInstance.SetValue(GenerateRandomObject(elementType), i);
                    }
                    property.SetValue(obj, arrayInstance);
                }
                else if (property.PropertyType.IsClass)
                {
                    property.SetValue(obj, GenerateRandomObject(property.PropertyType));
                }
                else
                {
                    throw new Exception($"暂不支持该类型的随机生成{property.PropertyType.FullName}");
                }
            }
            return obj;
        }

        public static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static char GenerateRandomChar()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return chars[random.Next(chars.Length)];
        }

        public static object GenerateRandomNumber(Type type)
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
