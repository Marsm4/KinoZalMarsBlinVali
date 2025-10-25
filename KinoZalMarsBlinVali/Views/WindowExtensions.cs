using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public static class WindowExtensions
    {
        public static Task<TResult> ShowDialog<TResult>(this Window window, Window parent)
        {
            var tcs = new TaskCompletionSource<TResult>();
            window.Closed += (s, e) =>
            {
                if (window.DataContext is TResult result)
                {
                    tcs.SetResult(result);
                }
                else
                {
                    tcs.SetResult(default);
                }
            };
            window.ShowDialog(parent);
            return tcs.Task;
        }
    }
}
