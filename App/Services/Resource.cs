using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Server;

namespace QServer.App.Services
{
    [QServer.Core.Service]
    internal class Resource: Server.Service
    {
        public Resource() : base(nameof(Resource).ToLowerInvariant())
        {
        }
        public override void Exec(RequestArgs args)
        {
            base.Exec(args);
        }
        public override bool Options(RequestArgs args)
        {
            return base.Options(args);
        }
        public override void Head(RequestArgs args)
        {
            base.Head(args);
        }
        public override bool Get(RequestArgs args)
        {
            var req = args.context.Request;

            if (!args.GetParam("file", out string resource)) return args.SendCode(System.Net.HttpStatusCode.BadRequest);
            var file = Api.Explorer.GetFileInfo(resource);
            if (!file.Exists) return args.SendCode(System.Net.HttpStatusCode.NotFound);

            return sendLargeFile(args, file, 0, file.Length);

        }

        private bool sendLargeFile1(RequestArgs args, FileInfo file, long start, long size)
        {
            size = Math.Min(start + size, file.Length) - start;
            var end = start + size - 1;

            var res = args.context.Response;
            
            
            res.ContentType = Server.Resource.GetContentType(file.Extension);
            res.AddHeader("Range-Unit", "items");
            res.AddHeader("Content-Range", $"{start}-{end}/{file.Length}");
            //res.AddHeader("Content-Length", size.ToString());
            //res.ContentLength64 = size;

            var output = res.OutputStream;
            var input = File.OpenRead(file.FullName);
            input.Seek(start, SeekOrigin.Begin);
            var buffer = new byte[size];
            var offset = 0;
            while (offset<buffer.Length)
                offset += input.Read(buffer, offset, buffer.Length - offset);
            res.StatusCode = 206;
            output.Write(buffer, 0, buffer.Length);
            res.Close();
            
            
            //args.IsBusy = true;
            return true;
        }
        private bool sendLargeFile(RequestArgs args, FileInfo file,long start,long size)
        {
            var maxBufferSize = 1000 * 10;
            size = Math.Min(start + size, file.Length) - start;
            var iterator = new SegmentIterator(size, maxBufferSize, start);
            var res = args.context.Response;
            var fSTR = File.OpenRead(file.FullName);
            fSTR.Seek(start, SeekOrigin.Begin);
            var nSTR = res.OutputStream;
            var dispose = false;
            
            byte[] buffer = new byte[maxBufferSize];
            void DisposeStream()
            {
                args.IsBusy = false;
                args.context.Response.Close();
                if (dispose)
                    args.Dispose();
                return ;
            }
            byte[] getNextBytes()
            {
                if (iterator.MoveNext())
                {
                    fSTR.Seek(iterator.CurrentOffset, SeekOrigin.Begin);
                    iterator.SetSegmentSize(fSTR.Read(buffer, 0, (int)iterator.CurrentSize));
                    return iterator.CurrentSize == 0 ? null : buffer;
                }
                DisposeStream();
                return null;
            }
            void write()
            {
                if (getNextBytes() == null) { DisposeStream(); return; }
                try
                {
                    nSTR.BeginWrite(buffer, 0,(int) iterator.CurrentSize, (a) =>
                    {
                        try { nSTR.EndWrite(a); }
                        catch { DisposeStream(); return; }
                        write();
                    }, this);
                }
                catch { DisposeStream(); }
            }
            
            var Response = args.context.Response;
            Response.ContentType = Server.Resource.GetContentType(file.Extension);
            //Response.AddHeader("Content-Disposition", $"attachment;filename=\"{file.Name}\"");
            //Response.AddHeader("Content-Range", $"bytes ${start}-${size - start - 1}/${file.Length}");
            //Response.AddHeader("Accept-Ranges", "bytes");
            //Response.AddHeader("Content-Length", size.ToString());

            /*        const head = {
                      'Content-Range': `bytes ${start}-${end}/${fileSize}`,
                      'Accept-Ranges': 'bytes',
                      'Content-Length': chunksize,
                      'Content-Type': 'video/mp4',
                    }*/
            args.IsBusy = true;
            iterator.Reset();
            write();
            dispose = true;
            return true;
        }
    }
    public class Segment
    {
        public readonly long Start;
        public long Size;
        private readonly long _offset;
        public long Offset => Start + _offset;
        public Segment(long start, long size,long offset)
        {
            Start = start;
            Size = size;
            _offset = offset;
        }
        public Segment Next(long size, long step)
        {
            var start = Start + Size;
            var rest = size - start;
            if (rest <= 0) return null;
            if (rest > step) rest = step;
            return new Segment(start, rest, _offset);
        }
    }
    public class SegmentIterator : IEnumerator<Segment>, IEnumerable<Segment>
    {
        public readonly long Size;
        public readonly long Offset;
        public readonly long BufferSize;

        public Segment Current { get; private set; }

        public long CurrentOffset => Current?.Offset ?? 0;
        public long CurrentSize => Current?.Size ?? 0;

        public SegmentIterator(long size, long step, long offset = 0)
        {
            BufferSize = step;
            Size = size;
            Offset = offset;
        }

        object IEnumerator.Current => Current;

        public void Dispose() => Current = null;

        public bool MoveNext() => (Current = Current?.Next(Size, BufferSize)) != null;

        public void Reset() => Current = new Segment(0, 0, Offset);

        public void SetSegmentSize(long size) => Current.Size = size;
        public bool ResetII { set { Reset(); } }
        public IEnumerator<Segment> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
/*
namespace test
{

    abstract class Test
    {
        #region Properties
        public string ContentType { get; private set; }

        public string FileName { get; private set; }

        public DateTime FileModificationDate { get; private set; }

        private DateTime HttpModificationDate { get; set; }

        public long FileLength { get; private set; }

        private string EntityTag { get; set; }

        private long[] RangesStartIndexes { get; set; }

        private long[] RangesEndIndexes { get; set; }

        private bool RangeRequest { get; set; }

        private bool MultipartRequest { get; set; }
        #endregion

        #region Constructor
        protected RangeFileResult(string contentType, string fileName, DateTime modificationDate, long fileLength)
        {
            if (String.IsNullOrEmpty(contentType))
                throw new ArgumentNullException("contentType");

            ContentType = contentType;
            FileName = fileName;
            FileLength = fileLength;
            FileModificationDate = modificationDate;
            //Modification date for header values comparisons purposes
            HttpModificationDate = modificationDate.ToUniversalTime();
            HttpModificationDate = new DateTime(HttpModificationDate.Year, HttpModificationDate.Month, HttpModificationDate.Day, HttpModificationDate.Hour, HttpModificationDate.Minute, HttpModificationDate.Second, DateTimeKind.Utc);
        }
        #endregion

        #region Methods
        protected virtual string GenerateEntityTag(ControllerContext context)
        {
            //Generate entity tag based on file name and modification date
            byte[] entityTagBytes = Encoding.ASCII.GetBytes(String.Format("{0}|{1}", FileName, FileModificationDate));
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(entityTagBytes));
        }
        #endregion
        #region Fields
        private static char[] _commaSplitArray = new char[] { ',' };
        private static char[] _dashSplitArray = new char[] { '-' };
        private static string[] _httpDateFormats = new string[] { "r", "dddd, dd-MMM-yy HH':'mm':'ss 'GMT'", "ddd MMM d HH':'mm':'ss yyyy" };
        #endregion
        private bool ValidateRanges(HttpResponseBase response)
        {
            if (FileLength > Int32.MaxValue)
            {
                response.StatusCode = 413;
                return false;
            }

            for (int i = 0; i < RangesStartIndexes.Length; i++)
            {
                if (RangesStartIndexes[i] > FileLength - 1 || RangesEndIndexes[i] > FileLength - 1 || RangesStartIndexes[i] < 0 || RangesEndIndexes[i] < 0 || RangesEndIndexes[i] < RangesStartIndexes[i])
                {
                    response.StatusCode = 400;
                    return false;
                }
            }

            return true;
        }
        //Helper method for getting HTTP headers values
        private string GetHeader(HttpRequestBase request, string header, string defaultValue = "")
        {
            return String.IsNullOrEmpty(request.Headers[header]) ? defaultValue : request.Headers[header].Replace("\"", String.Empty);
        }

        private void GetRanges(HttpRequestBase request)
        {
            //Get "Range" header from request
            string rangesHeader = GetHeader(request, "Range");
            //Get "If-Range" header from request
            string ifRangeHeader = GetHeader(request, "If-Range", EntityTag);
            DateTime ifRangeHeaderDate;
            bool isIfRangeHeaderDate = DateTime.TryParseExact(ifRangeHeader, _httpDateFormats, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out ifRangeHeaderDate);

            //If there is no "Range" header,
            //or the entity tag from "If-Range" header does not match this entity tag,
            //or the modification date is greater than modification date from "If-Range" header
            if (String.IsNullOrEmpty(rangesHeader) || (!isIfRangeHeaderDate && ifRangeHeader != EntityTag) || (isIfRangeHeaderDate && HttpModificationDate > ifRangeHeaderDate))
            {
                //Return entire file
                RangesStartIndexes = new long[] { 0 };
                RangesEndIndexes = new long[] { FileLength - 1 };
                RangeRequest = false;
                MultipartRequest = false;
            }
            //Otherwise
            else
            {
                //Split "Range" header value into ranges
                string[] ranges = rangesHeader.Replace("bytes=", String.Empty).Split(_commaSplitArray);

                RangesStartIndexes = new long[ranges.Length];
                RangesEndIndexes = new long[ranges.Length];
                RangeRequest = true;
                MultipartRequest = (ranges.Length > 1);

                //Get the star and end index for the range 
                for (int i = 0; i < ranges.Length; i++)
                {
                    string[] currentRange = ranges[i].Split(_dashSplitArray);

                    if (String.IsNullOrEmpty(currentRange[1]))
                        RangesEndIndexes[i] = FileLength - 1;
                    else
                        RangesEndIndexes[i] = Int64.Parse(currentRange[1]);

                    if (String.IsNullOrEmpty(currentRange[0]))
                    {
                        RangesStartIndexes[i] = FileLength - 1 - RangesEndIndexes[i];
                        RangesEndIndexes[i] = FileLength - 1;
                    }
                    else
                        RangesStartIndexes[i] = Int64.Parse(currentRange[0]);
                }
            }
        }
        

        protected abstract void WriteEntireEntity(HttpResponseBase response);

        protected abstract void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex);

        public override void ExecuteResult(ControllerContext context)
        {
            //Generate entity tag
            EntityTag = GenerateEntityTag(context);
            //Get ranges from request
            GetRanges(context.HttpContext.Request);

            //If all validations are successful
            if (ValidateRanges(context.HttpContext.Response) && ValidateModificationDate(context.HttpContext.Request, context.HttpContext.Response) && ValidateEntityTag(context.HttpContext.Request, context.HttpContext.Response))
            {
                //Set common headers
                context.HttpContext.Response.AddHeader("Last-Modified", FileModificationDate.ToString("r"));
                context.HttpContext.Response.AddHeader("ETag", String.Format("\"{0}\"", EntityTag));
                context.HttpContext.Response.AddHeader("Accept-Ranges", "bytes");

                //If this is not range request
                if (!RangeRequest)
                {
                    //Set standard headers
                    context.HttpContext.Response.AddHeader("Content-Length", FileLength.ToString());
                    context.HttpContext.Response.ContentType = ContentType;
                    //Set status code
                    context.HttpContext.Response.StatusCode = 200;

                    //If this is not HEAD request
                    if (!context.HttpContext.Request.HttpMethod.Equals("HEAD"))
                        //Write entire file to response
                        WriteEntireEntity(context.HttpContext.Response);
                }
                //If this is range request
                else
                {
                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

                    //Compute and set content length
                    context.HttpContext.Response.AddHeader("Content-Length", GetContentLength(boundary).ToString());

                    //If this is not multipart request
                    if (!MultipartRequest)
                    {
                        //Set content range and type
                        context.HttpContext.Response.AddHeader("Content-Range", String.Format("bytes {0}-{1}/{2}", RangesStartIndexes[0], RangesEndIndexes[0], FileLength));
                        context.HttpContext.Response.ContentType = ContentType;
                    }
                    //Otherwise
                    else
                        //Set proper content type
                        context.HttpContext.Response.ContentType = String.Format("multipart/byteranges; boundary={0}", boundary);

                    //Set status code
                    context.HttpContext.Response.StatusCode = 206;

                    //If this not HEAD request
                    if (!context.HttpContext.Request.HttpMethod.Equals("HEAD"))
                    {
                        //For each requested range
                        for (int i = 0; i < RangesStartIndexes.Length; i++)
                        {
                            //If this is multipart request
                            if (MultipartRequest)
                            {
                                //Write additional multipart info
                                context.HttpContext.Response.Write(String.Format("--{0}\r\n", boundary));
                                context.HttpContext.Response.Write(String.Format("Content-Type: {0}\r\n", ContentType));
                                context.HttpContext.Response.Write(String.Format("Content-Range: bytes {0}-{1}/{2}\r\n\r\n", RangesStartIndexes[i], RangesEndIndexes[i], FileLength));
                            }

                            //Write range (with multipart separator if required)
                            if (context.HttpContext.Response.IsClientConnected)
                            {
                                WriteEntityRange(context.HttpContext.Response, RangesStartIndexes[i], RangesEndIndexes[i]);
                                if (MultipartRequest)
                                    context.HttpContext.Response.Write("\r\n");
                                context.HttpContext.Response.Flush();
                            }
                            else
                                return;
                        }

                        //If this is multipart request
                        if (MultipartRequest)
                            context.HttpContext.Response.Write(String.Format("--{0}--", boundary));
                    }
                }
            }
        }

        //Helper method for computing content length
        private int GetContentLength(string boundary)
        {
            int contentLength = 0;

            for (int i = 0; i < RangesStartIndexes.Length; i++)
            {
                contentLength += Convert.ToInt32(RangesEndIndexes[i] - RangesStartIndexes[i]) + 1;

                if (MultipartRequest)
                    contentLength += boundary.Length + ContentType.Length + RangesStartIndexes[i].ToString().Length + RangesEndIndexes[i].ToString().Length + FileLength.ToString().Length + 49;
            }

            if (MultipartRequest)
                contentLength += boundary.Length + 4;

            return contentLength;
        }
    }
}*/