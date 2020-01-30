using System;
using System.IO;
using System.Net;
using System.Text;

namespace Homebrew
{
    /// <summary>
    /// Класс отвечает за создание и настройку нового запрос.
    /// <para>Не работает без инициализации конструктора
    /// <seealso cref="ReqParametres"/></para>
    /// </summary>
    class ReqParametres
    {
        public HttpWebRequest Request { get; }
        //Parametres
        private string _link;
        private HttpMethod _httpMethod;
        private string _reqData;
        private bool _allowAutoRedirect;
        private int _maximumAutomaticRedirections;
        private string _contentType;
        private CookieCollection _cookieCollection;
        private string _userAgent;

        /// <summary>
        /// Основной метод создания нового запроса.
        /// <para><seealso cref="SetUserAgent(string)"/> - установка Юзер-агента</para>
        /// <para><see cref="SetProxy(string, string, string)"/> - установка прокси</para>
        /// <para><see cref="SetReqAdditionalParametres(bool, int, string, CookieCollection)"/> - установка дополнительных параметров</para>
        /// </summary>
        /// <param name="link">Основная ссылка</param>
        /// <param name="httpMethod">Тип запроса (Get, Post,Put и т.д.)</param>
        /// <param name="reqData">Тело запроса</param>
        public ReqParametres(String link, HttpMethod httpMethod = HttpMethod.GET, String reqData = "")
        {

            //Set Link
            _link = LinkFormatter(link);
            Request = (HttpWebRequest)WebRequest.Create(_link);

            //SetMethod
            _httpMethod = httpMethod;
            Request.Method = _httpMethod.ToString();
            _reqData = reqData;
            if (_reqData.Length > 0)
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(_reqData);
                Request.ContentLength = byte1.Length;
                Stream newStream = Request.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
            }
        }
        /// <summary>
        /// Установка дополнительных параметров
        /// </summary>
        /// <param name="allowAutoRedirect">Включить авторедирект?</param>
        /// <param name="maximumAutomaticRedirections">Максимальное количество редиректов</param>
        /// <param name="contentType">Тип контента задаётся через класс "ParserContentType"</param>
        /// <param name="cookieCollection">Cookie задаются через "CookieCollection"</param>
        public void SetReqAdditionalParametres(bool allowAutoRedirect = true, int maximumAutomaticRedirections = 100,
            String contentType = "application/x-www-form-urlencoded", CookieCollection cookieCollection = null)
        {
            //Set AutoRedirect
            _allowAutoRedirect = allowAutoRedirect;
            _maximumAutomaticRedirections = maximumAutomaticRedirections;
            Request.MaximumAutomaticRedirections = _maximumAutomaticRedirections;
            Request.AllowAutoRedirect = _allowAutoRedirect;

            //ContentType
            _contentType = contentType;
            Request.ContentType = _contentType;

            //Cookie
            if (cookieCollection != null)
            {
                _cookieCollection = cookieCollection;
                Request.CookieContainer.Add(_cookieCollection);
            }
        }
        /// <summary>
        /// Установка прокси
        /// </summary>
        /// <param name="proxy">IP-адрес прокси</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        public void SetProxy()
        {
            SetTimout(5000);
            try
            {
                Request.Proxy = new WebProxy(new Uri("http://" + Proxies.GetProxy()));
            }
            catch (Exception ex)
            {
                SetProxy();
            }
        }
        /// <summary>
        /// Установка юзер-агента
        /// </summary>
        /// <param name="userAgent">Юзер-агент</param>
        public void SetUserAgent(string userAgent)
        {
            _userAgent = userAgent;
            Request.UserAgent = _userAgent;
        }
        public void SetTimout(int timeout)
        {

            Request.Timeout = timeout;
        }
        private static String LinkFormatter(String link)
        {
            String newLink = link;
            if (!newLink.StartsWith("http"))
            {
                if (!newLink.Contains("www."))
                {
                    newLink = $"www.{newLink}";
                }
                newLink = $"http://{newLink}";
            }
            return newLink;
        }
    }
}
