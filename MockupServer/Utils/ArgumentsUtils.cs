namespace MockupServer.Utils
{
    public class ArgumentsUtils
    {
        public static string GetValue(string[] args, string paramName)
        {
            foreach (var item in args)
            {
                Console.WriteLine(item);
            }
            var all = args.Where(x => x.IndexOf('=') != -1).ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1], StringComparer.OrdinalIgnoreCase);
            if (all.ContainsKey(paramName))
                return all[paramName];
            else if (all.ContainsKey($"--{paramName}"))
                return all[$"--{paramName}"];
            else
                return String.Empty;
        }
    }
}
