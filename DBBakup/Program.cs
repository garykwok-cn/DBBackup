using System;
using System.Collections.Generic;
using System.Text;

namespace DBBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            BackService backService = new BackService();
            backService.Start(args);
        }
    }
}
