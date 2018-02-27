using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserConsoleApp.ServiceReference;

namespace UserConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().FullName;
            string input = string.Empty;
            do
            {
                Console.Write("Your age: ");
                input = Console.ReadLine();
                var client = new UserServiceClient();
                switch (input)
                {
                    case "Q":
                    case "q":
                        break;
                    default:
                        client.MethodThatWillChangeData(int.Parse(input));
                        break;
                }
                Console.WriteLine();

            } while (input.ToLower() != "q");
        }
    }
}
