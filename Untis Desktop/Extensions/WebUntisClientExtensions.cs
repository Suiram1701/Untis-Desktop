using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using UntisDesktop.Localization;
using UntisDesktop.ViewModels;
using WebUntisAPI.Client;
using WebUntisAPI.Client.Exceptions;

namespace UntisDesktop.Extensions;

public static class WebUntisClientExtensions
{
    public static TResult? RunWithDefaultExceptionHandler<TResult>(this WebUntisClient client, Func<WebUntisClient, TResult> func, IWindowViewModel handlerViewModel, string logName)
        where TResult : Task
    {
        try
        {
            return func(client);
        }
        catch (WebUntisException ex)
        {
            switch (ex.Code)
            {
                case (int)WebUntisException.Codes.NoRightForMethod:
                    handlerViewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NRFM");
                    Logger.LogWarning($"{logName}: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NoRightForMethod)}");
                    break;
                case (int)WebUntisException.Codes.NotAuthticated:
                    handlerViewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU.NA");
                    Logger.LogWarning($"{logName}: {nameof(WebUntisException)} {nameof(WebUntisException.Codes.NotAuthticated)}");
                    break;
                default:
                    handlerViewModel.ErrorBoxContent = LangHelper.GetString("App.Err.WU", ex.Message);
                    Logger.LogError($"{logName}: Unexpected {nameof(WebUntisException)} Message: {ex.Message}, Code: {ex.Code}");
                    break;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.Source == "System.Net.Http" && ex.StatusCode is null)
                handlerViewModel.IsOffline = true;
            else
                handlerViewModel.ErrorBoxContent = LangHelper.GetString("App.Err.NERR", ex.Message, ((int?)ex.StatusCode)?.ToString() ?? "0");
            Logger.LogWarning($"{logName}: {nameof(HttpRequestException)} Code: {ex.StatusCode}, Message: {ex.Message}");
        }
        catch (Exception ex)
        {
            handlerViewModel.ErrorBoxContent = LangHelper.GetString("App.Err.OEX", ex.Source ?? "System.Exception", ex.Message);
            Logger.LogError($"{logName}: {ex.Source ?? "System.Exception"}; {ex.Message}");
        }

        return default;
        ;
    }
}