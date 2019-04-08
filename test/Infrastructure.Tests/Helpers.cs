using System;
using System.IO;
using System.Reflection;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Infrastructure.Tests
{
    public class Helpers
    {
        private static int _id = (int) 1e3;

        /// <summary>
        ///     Gets the full path to the target project that we wish to test
        /// </summary>
        /// <param name="projectRelativePath">
        ///     The parent directory of the target project.
        ///     e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        public static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = AppContext.BaseDirectory;

            // Find the path to the target project
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
                if (projectDirectoryInfo.Exists)
                {
                    var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName,
                        $"{projectName}.csproj"));
                    if (projectFileInfo.Exists) return Path.Combine(projectDirectoryInfo.FullName, projectName);
                }
            } while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        public static int GetUniqueId()
        {
            return Interlocked.Increment(ref _id);
        }

        public static void UsingDb(string connectionString, Action<MySqlConnection> action)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                action(connection);
            }
        }

        public static T UsingDb<T>(string connectionString, Func<MySqlConnection, T> func)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                return func(connection);
            }
        }
    }
}