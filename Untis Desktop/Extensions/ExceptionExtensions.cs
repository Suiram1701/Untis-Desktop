using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UntisDesktop.Localization;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client.Exceptions;

namespace UntisDesktop.Extensions;

internal static class ExceptionExtensions
{
    public static void HandleWithDefaultHandler(this Exception exception, IWindowViewModel handler, string logName)
    {
        if (exception is null)
            throw new ArgumentNullException(nameof(exception));

        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (logName is null)
            throw new ArgumentNullException(nameof(logName));

        if (exception is WebUntisException wuEx)
        {
            switch (wuEx.Code)
            {
                case (int)WebUntisException.Codes.NoRightForMethod:
                    handler.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NRFM");
                    Logger.LogWarning($"{logName}: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NoRightForMethod)}");
                    break;
                case (int)WebUntisException.Codes.NotAuthticated:
                    handler.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NA");
                    Logger.LogWarning($"{logName}: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NotAuthticated)}");
                    break;
                default:
                    handler.ErrorBoxContent = LangHelper.GetString("App.Err.WU", wuEx.Message);
                    Logger.LogError($"{logName}: Unexpected {nameof(WebUntisException)} Message: {wuEx.Message}, Code: {wuEx.Code}");
                    break;
            }
        }
        else if (exception is HttpRequestException httpEx)
        {
            if (httpEx.Source == "System.Net.Http" && httpEx.StatusCode is null)
                handler.IsOffline = true;
            else
                handler.ErrorBoxContent = LangHelper.GetString("App.Err.NERR", httpEx.Message, ((int?)httpEx.StatusCode)?.ToString() ?? "0");
            Logger.LogWarning($"{logName}: {nameof(HttpRequestException)} Code: {httpEx.StatusCode}, Message: {httpEx.Message}");
        }
        else if (exception.Source == "System.Net.Http")
        {
            handler.IsOffline = true;
        }
        else
        {
            handler.ErrorBoxContent = LangHelper.GetString("App.Err.OEX", exception.Source ?? "System.Exception", exception.Message);
            Logger.LogError($"{logName}: {exception.Source ?? "System.Exception"}; {exception.Message}");
        }
    }
}