using System;
using System.Text;
using System.Windows.Forms;

namespace common_forms.Logging
{
    /// <summary>
    /// A logging utility class that enables rich text formatting and asynchronous logging to a RichTextBox control.
    /// </summary>
    /// <remarks>
    /// The RichTextBoxLogger uses a background task to periodically update the text box to avoid freezing the UI during intensive logging operations.
    /// It supports text formatting tags like bold, underline, colored text... as defined in <see cref="TextTag"/>
    /// </remarks>
    public class RichTextBoxLogger : IDisposable, IRTBLogger
    {
        private readonly StringBuilder _sb;
        private readonly RichTextBoxTextFormatter _rtbTextFormater;
        private readonly Tags _tags;
        private Timer _timer;

        private bool doUpdate = false;
        private object _lock = new object();

        /// <summary>
        /// <inheritdoc cref="RichTextBoxLogger"/>
        /// </summary>
        /// <param name="rtbOutputRich">The RichTextBox control where log messages will be displayed.</param>
        /// <param name="updateInterval">The interval in milliseconds at which the text box is updated with new log entries.</param>
        public RichTextBoxLogger(RichTextBox rtbOutputRich, int updateInterval = 1000)
        {
            _rtbTextFormater = new RichTextBoxTextFormatter(rtbOutputRich);
            _tags = new Tags();
            _sb = new StringBuilder();
            _timer = new Timer
            {
                Interval = Math.Max(updateInterval, 1000)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public string Log(string message, TextTag tag, bool newSection)
        {
            var text = $"{Environment.NewLine}{_tags.WrapInTag(message, tag)}";
            return Log(text);
        }

        public string Log(string message, TextTag tag)
        {
            var text = _tags.WrapInTag(message, tag);
            return Log(text);
        }

        public string Log(string message, bool newSection)
        {
            message = $"{Environment.NewLine}{message}";
            return Log(message);
        }

        public string Log(string message)
        {
            lock (_lock)
            {
                doUpdate = true;
                if (string.IsNullOrEmpty(message))
                    _sb.AppendLine(string.Empty);
                else if (message.StartsWith(Environment.NewLine))
                {
                    _sb.AppendLine(string.Empty);
                    message = message.Substring(Environment.NewLine.Length);
                    _sb.AppendLine($"{DateTime.Now.ToString("HH:mm:ss")}  {message}");
                }
                else
                    _sb.AppendLine($"{DateTime.Now.ToString("HH:mm:ss")}  {message}");
            }
            return message;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        #region DISPLAY 
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (doUpdate)
            {
                _rtbTextFormater.DisplayAndFormatText(_sb.ToString());
                doUpdate = false;
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Clear()
        {
            _sb.Clear();
            doUpdate = true;
        }
        #endregion
    }
}
