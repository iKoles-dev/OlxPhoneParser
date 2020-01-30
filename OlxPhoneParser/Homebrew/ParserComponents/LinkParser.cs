using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Homebrew
{

    /// <summary>
    /// Парсер ссылок.
    /// Принимает HttpWebRequest в конструктор
    /// </summary>
    /// <seealso cref="ReqParametres"/>
    public class LinkParser
    {
        public CookieContainer Cookies { get; private set; }
        public CookieCollection CollCookies { get; private set; }
        public String Data { get; private set; }
        private HttpWebRequest request;
        private Encoding _encoding = Encoding.UTF8;
        public void SetEncodingMethod(Encoding encoding)
        {
            _encoding = encoding;
        }
        public LinkParser(HttpWebRequest httpRequest)
        {
            request = httpRequest;
            if (request.CookieContainer==null || request.CookieContainer.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
            }
            StartParsing();
        }
        public LinkParser(HttpWebRequest httpRequest, Encoding encoding)
        {
            _encoding = encoding;
            request = httpRequest;
            if (request.CookieContainer == null || request.CookieContainer.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
            }
            StartParsing();
        }
        private void StartParsing()
        {
            try
            {
                string data;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream;
                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, _encoding);
                    }
                    data = readStream.ReadToEnd();
                    CookieContainer cookieContainer = new CookieContainer();
                    foreach (Cookie cookie in response.Cookies)
                    {
                        cookieContainer.Add(cookie);
                    }
                    Cookies = cookieContainer;
                    CollCookies = response.Cookies;
                    response.Close();
                    readStream.Close();
                }
                else
                {
                    data = "";
                }
                Data = data;
            }
            catch (Exception error)
            {
                Data = "";
            }
        }
    }
}
