using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace EConfig.Tests
{
    public class InlineFileStreamAttribute : DataAttribute, IDisposable
    {
        private List<FileStream> streams = new List<FileStream>();

        public string[] FilePaths { get; set; }


        public InlineFileStreamAttribute(string[] path)
        {
            this.FilePaths = path;
        }

        public void Dispose()
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
            streams = null;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            foreach (var filePath in FilePaths)
            {
                var fullPath = Path.IsPathRooted(filePath)
                    ? filePath
                    : Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

                if (!File.Exists(fullPath))
                {
                    throw new ArgumentException($"Please specify a valid path! File {filePath} did not exist!");
                }

                streams.Add(File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite));
            }

            return new List<object[]> { streams.Cast<object>().ToArray() }.AsEnumerable();
        }
    }
}