using System;
using System.IO;

using FileAccess = CMS.IO.FileAccess;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;
using Stream = System.IO.Stream;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using AzureStorageProvider.Collections;

namespace AzureStorageProvider
{
    /// <summary>
    /// Implementation of file stream for Microsoft Azure.
    /// </summary>
    public class FileStream : CMS.IO.FileStream
    {
        private string _path;
        private FileMode _fileMode = FileMode.Open;
        private FileAccess _fileAccess = FileAccess.ReadWrite;
        private FileShare _fileShare = FileShare.Read;
        private int _bufferSize = 0;
        private Stream _stream = new MemoryStream();

        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(path));
        }
        


        #region "Constructors"

        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        public FileStream(string path, FileMode mode)
            : this(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite)
        {
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public FileStream(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, FileShare.Read)
        {
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>       
        /// <param name="share">Sharing permissions.</param>
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, 0x1000)
        {
        }


        /// <summary>
        /// Initializes new instance and intializes new system file stream.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        /// <param name="bSize">Buffer size.</param>
        /// <param name="share">Sharing permissions.</param>
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bSize)
            : base(path)
        {
            _path = path;
            _fileMode = mode;
            _fileAccess = access;
            _fileShare = share;
            _bufferSize = bSize;

            InitFileStream();
        }

        #endregion


        #region "Public properties"
        public override bool CanSeek => _stream.CanSeek;
        public override long Length => _stream.Length;
        public override long Position
        {
            get
            {
                return _stream.Position;
            }
            set
            {
                _stream.Position = value;
            }
        }
        
        public override bool CanRead => _stream.CanRead;
        public override bool CanWrite => _stream.CanWrite;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Reads data from stream and stores them into array.
        /// </summary>
        /// <param name="array">Array where result is stored.</param>
        /// <param name="offset">Offset from begining of file.</param>
        /// <param name="count">Number of characters which are read.</param>
        public override int Read(byte[] array, int offset, int count)
        {
            return _stream.Read(array, offset, count);
        }


        /// <summary>
        /// Closes current stream.
        /// </summary>
        public override void Close()
        {
            Dispose(true);

            _stream.Close();
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if (_fileAccess == FileAccess.Write || _fileAccess == FileAccess.ReadWrite)
            {
                _stream.Flush();
            }
        }


        /// <summary>
        /// Writes sequence of bytes to stream.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }


        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Location</param>
        public override long Seek(long offset, SeekOrigin loc)
        {
            return _stream.Seek(offset, loc);
        }


        /// <summary>
        /// Set length to stream.
        /// </summary>
        /// <param name="value">Value to set.</param>
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }


        /// <summary>
        /// Writes byte to the stream.
        /// </summary>
        /// <param name="value">Value to write.</param>
        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        #endregion


        #region IDisposable Members

        /// <summary>
        /// Releases all resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // handle closed streams
            if (!_stream.CanSeek)
            {
                // stream has been already disposed
                return;
            }

            Flush();

            if (_fileAccess == FileAccess.Write || _fileAccess == FileAccess.ReadWrite)
            {
                Seek(0, SeekOrigin.Begin);

                BlobHelper.Get(Path).Upload(_stream);

                SynchronizationHelper.LogUpdateFileTask(Path);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Initializes the stream
        /// </summary>
        protected virtual void InitFileStream()
        {
            var blob = BlobHelper.Get(_path);
            if (_fileMode == FileMode.CreateNew && blob.Exists())
                throw new Exception(string.Format("[FileStream.InitFileStream()]: Cannot create a new file '{0}', the file '{1}' already exists in BLOB storage.", Path, blob.Path));

            if (_fileMode != FileMode.Create && blob.Exists())
            {
                var data = blob.Get();
                Write(data, 0, data.Length);
                Seek(0, SeekOrigin.Begin);

                if (_fileMode == FileMode.Append)
                    Seek(data.Length, SeekOrigin.Begin);
            }

            if ((_fileMode == FileMode.Open || _fileMode == FileMode.Append) && !blob.Exists())
            {
                // weird fallback from original implementation
                // if file does not exist, take local file instead
                // should be absolutely removed!!!

                if (!System.IO.File.Exists(_path))
                    return;

                var data = System.IO.File.ReadAllBytes(_path);
                Write(data, 0, data.Length);
                Seek(0, SeekOrigin.Begin);

                if (_fileMode == FileMode.Append)
                    Seek(data.Length, SeekOrigin.Begin);
            }
        }
        #endregion

    }
}
