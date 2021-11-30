using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace myOwnWebServer
{
    class ResponseHeader
    {
        private const string httpVersion = "HTTP/1.1";
        public string ContentPath = "";
        public bool ValidRequest = false;

        private string FirstLine = "";
        public string StatusCode;
        public string ContentType = "text/html"; // Default, as any error page will be sent as HTML
        public string ServerDetails = Environment.MachineName; // Does not save actual server, but machine name of server
        public string ReplyDate = DateTime.Now.ToString();
        public int ContentLength = 0;


        //      METHOD: MakeHeader
        // DESCRIPTION: Takes the HTTP request as a string and splits it, processes each field in the header and returns the assembled header string
        public string GenerateHeader(string request, string root)
        {
            string[] splitString = request.Split(' ');

            ContentPath = root + splitString[1];

            if (GetStatusCode(splitString))
            {
                ValidRequest = true;
            }

            FirstLine = GetHTTPVersion(splitString[2]) + " " + StatusCode;
            ContentType = GetMIME(ContentPath);
            ContentLength = GetContentLength(ContentPath);

            return FirstLine + "\r\nContent-Type: " + ContentType + "\r\nServer: " + ServerDetails + "\r\nDate: " + ReplyDate + "\r\nContent-Length: " + ContentLength + "\r\n\r\n";
        }

        //      METHOD: GetContentLength
        // DESCRIPTION: Determines if the file exists and gets the length of the file. If file does not exist, processes HTML error page and recursively calls itself to get that error page's length
        private int GetContentLength(string dir)
        {
            string buffer = null;
            int len = 0;

            if (File.Exists(dir))
            {
                using (StreamReader sr = File.OpenText(dir))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        buffer += s;
                    }
                }
            }
            else
            {
                len = GetHTML(StatusCode).Length;
            }

            if (buffer != null)
            {
                len = buffer.Length;
            }
            return len;
        }

        //      METHOD: GetHTTPVersion
        // DESCRIPTION: Cuts out end of split string element to return HTTP version only
        private string GetHTTPVersion(string version)
        {
            version = version.Remove(version.IndexOf("\r\n"));
            return version;
        }

        // METHOD: GetStatusCode
        // DESCRIPTION: An incomplete status code function that checks through the different helper functions and returns the appropriate status code string
        private bool GetStatusCode(string[] splitString)
        {
            bool retCode = false;

            if (CheckGET(splitString[0]))
            {
                StatusCode = "501 Not Implemented";
            }
            else if (CheckHTTPVer(GetHTTPVersion(splitString[2])))
            {
                StatusCode = "505 HTTP Version Not Supported";
            }
            else if (CheckContentExists(ContentPath))
            {
                StatusCode = "404 Not Found";
            }
            else if (CheckMime(GetMIME(ContentPath)))
            {
                StatusCode = "415 Unsupported Media Type";
            }
            else
            {
                StatusCode = "200 OK";
                retCode = true;
            }
            return retCode;
        }

        // METHOD: CheckContentExists
        // DESCRIPTION: Checks if file in the path exists, if it does, return false
        private bool CheckContentExists(string path)
        {

            return !File.Exists(path);
        }

        // METHOD: GetMIME
        // DESCRIPTION: Checks extension for MIME type, if it exists, returns type, otherwise returns blank
        private string GetMIME(string dir)
        {
            string[] kHTMLExtensions = { ".htm", ".html", ".htmls" };
            string[] kJpegExtensions = { ".jfif", ".jpe", ".jpeg", ".jpg", ".jfif-tbnl" };
            const string kGifExtension = ".gif";
            const string kTxtExtension = ".txt";

            string ext = Path.GetExtension(dir);
            string retCode = "";

            if (ext == kGifExtension) // Is .gif?
            {
                retCode = "image/gif";
                ValidRequest = true;
            }
            else if (ext == kTxtExtension) // Is .txt
            {
                retCode = "text/plain";
                ValidRequest = true;
            }
            else
            {
                for (int i = 0; i < kHTMLExtensions.Length; i++)
                {
                    if (kHTMLExtensions[i] == ext) // Any type of html file?
                    {
                        retCode = "text/html";
                        ValidRequest = true;
                        break;
                    }

                }
                for (int ii = 0; ii < kJpegExtensions.Length; ii++) // Any type of Jpeg?
                {
                    if (kJpegExtensions[ii] == ext)
                    {
                        retCode = "image/jpeg";
                        ValidRequest = true;
                        break;
                    }
                }
            }
            return retCode;
        }

        // METHOD: ChecksMime
        // DESCRIPTION: Checks if mime is null or empty
        private bool CheckMime(string mime)
        {
            return string.IsNullOrEmpty(mime);
        }

        // METHOD: CheckHTTPVer
        // DESCRIPTION: Checks if version string is equal to expected constant, returns true if it isn't
        private bool CheckHTTPVer(string v)
        {
            return v != httpVersion;
        }

        // METHOD: CheckGET
        // DESCRIPTION: Checks first element in split array for GET, returns true if NOT GET
        private bool CheckGET(string v)
        {
            return v != "GET";
        }

        public static string GetHTML(string statusCode)
        {
            if (statusCode.StartsWith("404")) // NOT FOUND
            {
                return "<!DOCTYPE html><html>" +
                    "<head><title>404 Not Found</title></head>" +
                    "<body><h1>404: NOT FOUND</h1>" +
                    "<p><img src=\"../Images/404notfound.jpg\" alt=\"uhoh\"></p>" +
                    "<br/><hr>" + "<p>Use a file name that actually works.</p>" +
                    "</body></html>";
            }
            if (statusCode.StartsWith("415")) 
            {
                return "<!DOCTYPE html><html>" +
                    "<head><title>415: Media Unsupported</title></head>" +
                    "<body><h1>415: Unsupported Media Type</h1>" + "<p>You'll need to use a supported type.</p>" +
                    "<br/><hr>" + "<p><img src=\"../Images/noway.jpg\" alt=\"noway\">It's gotta end with </p>" + "<p>.html, .htmls, .htm, .gif, .txt, .jfif, .jpg, .jpe, .jpeg,  .jfif-tbnl </p>" + "<p></p>" +
                    "</body></html>";
            }
            if (statusCode.StartsWith("501")) 
            {
                return "<!DOCTYPE html><html>" +
                       "<head><title>501: That's Embarassing</title></head>" +
                       "<body><h1>501: Sorry, that's not implemented.</h1>" + "<p><img src=\"../Images/yikes.jpg\" alt=\"yikes\"></p>" + "<br/><hr>" + "<p>You can only use GET!</p>" +
                       "</body></html>";
            }
            if (statusCode.StartsWith("505")) 
            {
                return "<!DOCTYPE html><html>" + "<head><title>505 HTTP Version Not Supported</title></head>" + 
                       "<body><h1>505: HTTP Version Not Supported</h1>" + 
                       "<p>Try using a version that's actually supported.</p>" + "<br/><hr>" + "<p><img src=\"../Images/wrong.jpg\" alt=\"wrong\"></p>" + 
                       "</body></html>";
            }
            return "";
        }
    }
}
