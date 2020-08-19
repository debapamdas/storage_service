using SharpCompress.Compressors.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nokia.Storage.IntergrationTest
{
    public class TestFile
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public int Length { get; set; }

        // creating a new file with tar extension and content of the tar file
        // is same as file name(guid)
        public static TestFile CreateNewFile()
        {
            string guid = Guid.NewGuid().ToString();

            TestFile file = new TestFile()
            {
                ContentType = "application/x-tar",
                FileName = $"{guid}.tar",
                Content = guid
            };

            return file;
        }
    }
}
