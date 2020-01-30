#define WPF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
#if WPF
using System.Windows.Controls;
using System.Windows.Documents;
#endif
#if WP
using System.Windows.Forms;
#endif

namespace Homebrew
{
    public static class WorkHelper
    {
        /// <summary>
        /// Модуль парсинга "от" первого указанного значения и "до" второго указанного значения
        /// </summary>
        /// <param name="parsFrom">Значение от которого нужно парсить.</param>
        /// <param name="parsTo">Значение до которого нужно парсить.</param>
        /// <param name="includeFirst">Нужно ли оставлять значение, ОТ которого производился парсинг?</param>
        /// <param name="includeLast">Нужно ли оставлять значение, ДО которого производился парсинг?</param> 
        /// <returns>Возвращает пустую строку, если ничего не нашёл.</returns>
        public static String ParsFromTo(this string line, string parsFrom, string parsTo, bool includeFirst = false, bool includeLast = false)
        {
            if (line.Length != 0)
            {
                //Находим первое вхождение для парсинга "от"
                int exParsFrom = line.IndexOf(parsFrom);
                if (exParsFrom > -1)
                {
                    //Добавляем длину первого вхождения
                    exParsFrom = includeFirst ? exParsFrom : exParsFrom + parsFrom.Length;
                    //Устанавливаем строку, начиная от значения "от"
                    string endOfLine = line.Substring(exParsFrom, line.Length - exParsFrom);
                    //Находим вхождение "до" в строке endOfLine
                    int exParsTo = endOfLine.IndexOf(parsTo);
                    if (exParsTo > -1)
                    {
                        //Добавляем длину второго вхождения
                        exParsFrom = includeLast ? exParsFrom + parsTo.Length : exParsFrom;
                        return line.Substring(exParsFrom, exParsTo);
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// Модуль парсинга "от" первого указанного значения и до конца строки
        /// </summary>
        /// <param name="parsFrom">Значение от которого нужно парсить.</param>
        /// <param name="includeFirst">Нужно ли оставлять значение, ОТ которого производился парсинг?</param>
        /// <returns>Возвращает пустую строку, если ничего не нашёл.</returns>
        public static String ParsFromToEnd(this string line, string parsFrom, bool includeFirst = false)
        {
            if (line.Length != 0)
            {
                //Находим первое вхождение для парсинга "от"
                int exParsFrom = line.IndexOf(parsFrom);
                if (exParsFrom > -1)
                {
                    //Добавляем длину первого вхождения
                    exParsFrom = includeFirst ? exParsFrom : exParsFrom + parsFrom.Length;
                    return line.Substring(exParsFrom, line.Length - exParsFrom);
                }
            }
            return "";
        }
        /// <summary>
        /// Модуль парсинга по регулярному выражению
        /// </summary>
        /// <param name="regularExpression">Регулярное выражение</param>
        /// <param name="group">Группа для парсинга</param>
        /// <returns>Возвращает результат в виде List<string></returns>
        public static List<string> ParsRegex(this string line, string regularExpression, int group = 0)
        {
            List<String> rawDataList = new List<string>();
            Regex regex = new Regex(regularExpression);
            MatchCollection matches = regex.Matches(line);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                    rawDataList.Add(match.Groups[group].Value);
            }
            return rawDataList;
        }
#if WP
        public static void setInvoke(this RichTextBox richTextBox, string text)
        {
            richTextBox.BeginInvoke(new Action(() => { richTextBox.Text = text; }));
        }
        public static void setInvoke(this Label label, string text)
        {
            label.BeginInvoke(new Action(() => { label.Text = text; }));
        }
        public static void addInvoke(this RichTextBox richTextBox, string text)
        {
            richTextBox.BeginInvoke(new Action(() => 
            {

                richTextBox.HideSelection = false;
                richTextBox.AppendText(text); 
            }));
        }
#endif
#if WPF
        public static void Set(this RichTextBox richTextBox, string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                richTextBox.Document.Blocks.Clear();
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }));
        }
        //public static void setInvoke(this Label label, string text)
        //{
        //    label.BeginInvoke(new Action(() => { label.Text = text; }));
        //}
        public static void Write(this RichTextBox richTextBox, string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                richTextBox.AppendText(text);
                richTextBox.ScrollToEnd();
            }));
        }
        public static void WriteLine(this RichTextBox richTextBox, string text)
        {
            Write(richTextBox, text + "\n");
        }
#endif


    }
}
