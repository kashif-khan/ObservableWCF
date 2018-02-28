using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
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
            var client = new UserServiceClient();
            do
            {
                Console.Write("Your age: ");
                input = Console.ReadLine();
                switch (input)
                {
                    case "i":
                        break;
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
