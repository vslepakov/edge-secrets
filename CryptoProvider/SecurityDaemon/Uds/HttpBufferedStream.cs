// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeSecrets.CryptoProvider.SecurityDaemon.Uds
{
    internal class HttpBufferedStream : Stream
    {
        private const char CR = '\r';
        private const char LF = '\n';
        private readonly BufferedStream innerStream;

        public HttpBufferedStream(Stream stream)
        {
            this.innerStream = new BufferedStream(stream);
        }

        public override bool CanRead => this.innerStream.CanRead;

        public override bool CanSeek => this.innerStream.CanSeek;

        public override bool CanWrite => this.innerStream.CanWrite;

        public override long Length => this.innerStream.Length;

        public override long Position
        {
            get => this.innerStream.Position;
            set => this.innerStream.Position = value;
        }

        public override void Flush() => this.innerStream.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => this.innerStream.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => this.innerStream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.innerStream.ReadAsync(buffer, offset, count, cancellationToken);

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            int position = 0;
            byte[] buffer = new byte[1];
            bool crFound = false;
            var builder = new StringBuilder();
            while (true)
            {
                int length = await this.innerStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (length == 0)
                {
                    throw new IOException("Unexpected end of stream.");
                }

                if (crFound && (char)buffer[position] == LF)
                {
                    builder.Remove(builder.Length - 1, 1);
                    return builder.ToString();
                }

                builder.Append((char)buffer[position]);
                crFound = (char)buffer[position] == CR;
            }
        }

        public override long Seek(long offset, SeekOrigin origin) => this.innerStream.Seek(offset, origin);

        public override void SetLength(long value) => this.innerStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => this.innerStream.Write(buffer, offset, count);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.innerStream.WriteAsync(buffer, offset, count, cancellationToken);

        protected override void Dispose(bool disposing) => this.innerStream.Dispose();
    }
}
