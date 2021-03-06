using System;
using System.IO;
using NUnit.Framework;

namespace Waves.Utils.Tests.Nuget.Packer
{
    /// <summary>
    /// Packer tests.
    /// </summary>
    public class Tests
    {
        private string _workingDirectory;
        
        /// <summary>
        /// Sets up tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var tmp = Directory
                .GetParent(Environment.CurrentDirectory)?
                .Parent?
                .Parent?
                .Parent?.FullName;
            
            if (tmp == null) return;

            _workingDirectory = Path.Combine(tmp, "tests", "data");
        }

        /// <summary>
        /// Test that packer finish work successfully.
        /// </summary>
        [Test]
        public void Packer_Test_Pass()
        {
            var args = new string[]
            {
                "-Version",
                "0.0.0.0-test",
                "-NuGetExePath",
                Path.Combine(_workingDirectory, "files", "nuget.exe"),
                "-WorkingPath",
                _workingDirectory,
                "-OutputDirectory",
                Path.Combine(_workingDirectory, "bin", "packages"),
                "-Properties",
                "Configuration=Release"
            };

            var result = false;
            
            try
            {
                Utils.Nuget.Packer.Program.Main(args);

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            
            if (result) 
                Assert.Pass();
            else 
                Assert.Fail();
        }
    }
}