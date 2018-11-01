using System;
using System.IO;
using System.IO.Compression;

namespace Server.Zip
{
    public class  DeflateWrapperStream : DeflateStream, ICloseEx, IRequestLifetimeTracker
    {
        public DeflateWrapperStream(Stream stream, CompressionMode mode)
            : base(stream, mode, false)
        {
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            ICloseEx closeEx = BaseStream as ICloseEx;
            if (closeEx != null)
                closeEx.CloseEx(closeState);
            else
                Close();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback,
            object state)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (size >= 0)
            {
                if (size <= buffer.Length - offset)
                {
                    try
                    {
                        return base.BeginRead(buffer, offset, size, callback, state);
                    }
                    catch (Exception ex)
                    {
                        MyConsole.WriteLine(ex.Message);
                        try
                        {
                            if (GZipWrapperStream.IsFatal(ex))
                            {
                                throw;
                            }
                            if (!(ex is InvalidDataException) && !(ex is InvalidOperationException))
                            {
                                if (!(ex is IndexOutOfRangeException))
                                    goto label_15;
                            }
                            Close();
                        }
                        catch(Exception e)
                        {
                            MyConsole.WriteLine(e.Message);
                        }
                        label_15:
                        throw ex;
                    }
                }
            }
            throw new ArgumentOutOfRangeException("size");
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            try
            {
                return base.EndRead(asyncResult);
            }
            catch (Exception ex)
            {
                try
                {
                    if (GZipWrapperStream.IsFatal(ex))
                    {
                        throw;
                    }
                    if (!(ex is InvalidDataException) && !(ex is InvalidOperationException))
                    {
                        if (!(ex is IndexOutOfRangeException))
                            goto label_10;
                    }
                    Close();
                }
                catch
                {
                }
                label_10:
                throw ex;
            }
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (size >= 0)
            {
                if (size <= buffer.Length - offset)
                {
                    try
                    {
                        return base.Read(buffer, offset, size);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (GZipWrapperStream.IsFatal(ex))
                            {
                                throw;
                            }
                            if (!(ex is InvalidDataException) && !(ex is InvalidOperationException))
                            {
                                if (!(ex is IndexOutOfRangeException))
                                    goto label_15;
                            }
                            Close();
                        }
                        catch
                        {
                        }
                        label_15:
                        throw ex;
                    }
                }
            }
            throw new ArgumentOutOfRangeException("size");
        }

        void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
        {
            (BaseStream as IRequestLifetimeTracker).TrackRequestLifetime(requestStartTimestamp);
        }
    }
}