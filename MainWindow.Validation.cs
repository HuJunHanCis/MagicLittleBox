using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MagicLittleBox
{
    public partial class MainWindow
    {
        #region CHECK检查区域

        private int? CheckValidFreq(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out var freq))
            {
                return null;
            }

            if (freq < 4 || freq > 1000)
            {
                return null;
            }

            if (freq % 4 != 0)
            {
                var nearest = (int)(Math.Round(freq / 4.0, MidpointRounding.AwayFromZero) * 4);
                freq = nearest;
                textBox.Text = freq.ToString();
            }

            return freq;
        }

        private int? CheckValidPort(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || !int.TryParse(text, out var port))
            {
                return null;
            }

            if (port <= 0 || port >= 65535)
            {
                return null;
            }

            return port;
        }

        private string CheckValidAddr(TextBox textBox)
        {
            if (textBox == null)
            {
                return null;
            }

            var text = textBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var pattern = @"^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$";
            return Regex.IsMatch(text, pattern) ? text : null;
        }

        #endregion
    }
}
