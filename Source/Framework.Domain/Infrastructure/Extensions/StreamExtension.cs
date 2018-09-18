using System.IO;
using System.Text;

namespace Framework.Domain.Infrastructure.Extensions
{
    public static class StreamExtension
    {

        public static void ReadFromAnotherStream(this Stream stream, Stream source)
        {
            CopyStream(source, stream);
        }

        public static void ReadFromAnotherStreamAndFlush(this Stream stream, Stream source)
        {
            CopyStream(source, stream);
            stream.Flush();
            stream.Position = 0;
        }

        public static string ReadToEnd(this Stream stream)
        {
            var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            return reader.ReadToEnd();
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

    }
}
