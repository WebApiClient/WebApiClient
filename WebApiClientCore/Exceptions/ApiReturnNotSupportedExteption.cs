﻿using System;
using System.ComponentModel;
using System.Net;

namespace WebApiClientCore.Exceptions
{
    /// <summary>
    /// 表示接口不支持处理响应消息的异常
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ApiReturnNotSupportedExteption : ApiException, IStatusCodeException
    {
        /// <summary>
        /// 获取请求上下文
        /// </summary>
        public ApiResponseContext Context { get; }

        /// <summary>
        /// 获取异常提示信息
        /// </summary>
        public override string Message
        {
            get
            {
                var contentType = this.Context.HttpContext.ResponseMessage?.Content.Headers.ContentType?.ToString() ?? "<null>";
                return Resx.unsupported_ContentType.Format(contentType, this.Context.ActionDescriptor.Return.DataType.Type);
            }
        }

        /// <summary>
        /// 接口不支持处理响应消息的异常
        /// </summary>
        /// <param name="context"></param> 
        /// <exception cref="ArgumentNullException"></exception>
        public ApiReturnNotSupportedExteption(ApiResponseContext context)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 获取响应状态码
        /// </summary>
        HttpStatusCode? IStatusCodeException.GetStatusCode()
        {
            return this.Context.HttpContext.ResponseMessage?.StatusCode;
        }
    }
}
