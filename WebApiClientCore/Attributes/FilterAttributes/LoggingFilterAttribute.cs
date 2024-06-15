﻿using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebApiClientCore.HttpContents;

namespace WebApiClientCore.Attributes
{
    /// <summary>
    /// 表示将请求和响应内容的输出为日志的过滤器
    /// </summary>
    public class LoggingFilterAttribute : ApiFilterAttribute
    {
        private static readonly Action<ILogger, LogMessage, Exception> logError = LoggerMessage.Define<LogMessage>(LogLevel.Error, 0, "{LogMessage}");
        private static readonly Action<ILogger, LogMessage, Exception?> logInformation = LoggerMessage.Define<LogMessage>(LogLevel.Information, 1, "{LogMessage}");

        /// <summary>
        /// 获取或设置是否输出请求内容
        /// </summary>
        public bool LogRequest { get; set; } = true;

        /// <summary>
        /// 获取或设置是否输出响应内容
        /// </summary>
        public bool LogResponse { get; set; } = true;

        /// <summary>
        /// 将请求和响应内容的输出为日志的过滤器
        /// </summary>
        public LoggingFilterAttribute()
        {
            this.OrderIndex = int.MaxValue;
        }

        /// <summary>
        /// 请求之前
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public sealed async override Task OnRequestAsync(ApiRequestContext context)
        {
            if (context.HttpContext.HttpApiOptions.UseLogging == false)
            {
                return;
            }

            var logMessage = new LogMessage
            {
                RequestTime = DateTime.Now,
                HasRequest = this.LogRequest
            };

            if (this.LogRequest == true)
            {
                var request = context.HttpContext.RequestMessage;
                logMessage.RequestHeaders = request.GetHeadersString();
                logMessage.RequestContent = await ReadRequestContentAsync(request).ConfigureAwait(false);
            }

            context.Properties.Set(typeof(LoggingFilterAttribute), logMessage);
        }

        /// <summary>
        /// 响应后
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public sealed async override Task OnResponseAsync(ApiResponseContext context)
        {
            if (context.HttpContext.HttpApiOptions.UseLogging == false)
            {
                return;
            }

            var logMessage = context.Properties.Get<LogMessage>(typeof(LoggingFilterAttribute));
            if (logMessage == null)
            {
                return;
            }

            logMessage.ResponseTime = DateTime.Now;
            logMessage.Exception = context.Exception;

            var response = context.HttpContext.ResponseMessage;
            if (this.LogResponse && response != null)
            {
                logMessage.HasResponse = true;
                logMessage.ResponseHeaders = response.GetHeadersString();
                logMessage.ResponseContent = await ReadResponseContentAsync(context).ConfigureAwait(false);
            }

            await this.WriteLogAsync(context, logMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// 读取请求内容
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static async Task<string?> ReadRequestContentAsync(HttpApiRequestMessage request)
        {
            if (request.Content == null)
            {
                return null;
            }

            return request.Content is ICustomHttpContentConvertable conversable
                ? await conversable.ToCustomHttpContext().ReadAsStringAsync().ConfigureAwait(false)
                : await request.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 读取响应内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task<string?> ReadResponseContentAsync(ApiResponseContext context)
        {
            var content = context.HttpContext.ResponseMessage?.Content;
            if (content == null)
            {
                return null;
            }

            if (content.IsBuffered() == true || context.GetCompletionOption() == HttpCompletionOption.ResponseContentRead)
            {
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return "...";
        }

        /// <summary>
        /// 写日志到指定日志组件
        /// 默认写入Microsoft.Extensions.Logging
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="logMessage">日志消息</param>
        /// <returns></returns>
        protected virtual Task WriteLogAsync(ApiResponseContext context, LogMessage logMessage)
        {
            var logger = context.GetLogger();
            if (logger != null)
            {
                this.WriteLog(logger, logMessage);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 写日志到ILogger
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="logMessage">日志消息</param>
        protected virtual void WriteLog(ILogger logger, LogMessage logMessage)
        {
            if (logMessage.Exception == null)
            {
                logInformation(logger, logMessage, null);
            }
            else
            {
                logError(logger, logMessage.ToExcludeException(), logMessage.Exception);
            }
        }
    }
}
