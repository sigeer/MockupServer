using System.Reflection;

namespace MockupServer.Response
{
    public class ResponseUtility
    {
        static Random random = new Random();

        public static object GenerateRandomObject(Type responseType)
        {
            object obj = Activator.CreateInstance(responseType);

            // 获取所有公共属性
            PropertyInfo[] properties = responseType.GetProperties();

            // 为每个属性赋予随机字符的值
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsClass)
                {
                    property.SetValue(obj, GenerateRandomObject(property.PropertyType));
                }
                else if (property.PropertyType == typeof(string))
                {
                    // 生成随机字符串并赋值给属性
                    string randomValue = GenerateRandomString(10); // 生成长度为 10 的随机字符串
                    property.SetValue(obj, randomValue);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(obj, DateTime.Now.Ticks % 2 == 0);
                }
                else
                {
                    property.SetValue(obj, GenerateRandomNumber(property.PropertyType));
                }
            }
            return obj;
        }

        static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static object GenerateRandomNumber(Type type)
        {
            if (type == typeof(int))
            {
                return random.Next();
            }
            else if (type == typeof(double))
            {
                return random.NextDouble() * random.Next();
            }
            else if (type == typeof(decimal))
            {
                byte scale = (byte)random.Next(29);
                return new decimal(random.Next(), random.Next(), random.Next(), false, scale);
            }
            else if (type == typeof(long))
            {
                byte[] buffer = new byte[8];
                random.NextBytes(buffer);
                return BitConverter.ToInt64(buffer, 0);
            }
            else
            {
                return null;
            }
        }
    }
}
