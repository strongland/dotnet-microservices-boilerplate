using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace PipefyBackend {
    public static class Log {

        public static void Info(string message, string ?tag = null) {
            if (tag != null) message = $"{message} - {tag}";
            Console.WriteLine($"INFO: - {message}");
        }

        public static void Debug(string message, string ?tag = null) {
            if (tag != null) message = $"{message} - {tag}";
            Console.WriteLine($"DEBUG: - {message}");
        }

        public static void Trace(string message, string ?tag = null) {
            if (tag != null) message = $"{message} - {tag}";
            Console.WriteLine($"TRACE: - {message}");
        }

        public static void Ex(Exception ex) {
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null) { System.Console.WriteLine(ex.InnerException.Message); }
		}
    }
}
