﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApiClientCore.HttpContents
{
    /// <summary>
    /// 表示 multipart/form-data 表单
    /// </summary>
    public class FormDataContent : MultipartContent, ICustomHttpContentConvertable
    {
        /// <summary>
        /// 分隔符
        /// </summary>
        private readonly string boundary;

        /// <summary>
        /// 获取对应的ContentType
        /// </summary>
        public static string MediaType => "multipart/form-data";

        /// <summary>
        /// multipart/form-data 表单
        /// </summary>
        public FormDataContent()
            : this(Guid.NewGuid().ToString())
        {
        }

        /// <summary>
        /// multipart/form-data 表单
        /// </summary>
        /// <param name="boundary">分隔符</param>
        public FormDataContent(string boundary)
            : base("form-data", boundary)
        {
            this.boundary = boundary;
            var parameter = new NameValueHeaderValue("boundary", boundary);             
            this.Headers.ContentType!.Parameters.Clear();
            this.Headers.ContentType!.Parameters.Add(parameter);
        }

        /// <summary>
        /// 添加 httpContent
        /// </summary>
        /// <param name="content"></param>
        public override void Add(HttpContent content)
        {
            this.EnsureNotBuffered();
            base.Add(content);
        }

        /// <summary>
        /// 转换为自定义内容的HttpContent
        /// </summary>
        /// <returns></returns>
        public HttpContent ToCustomHttpContext()
        {
            var customHttpContent = new FormDataContent(this.boundary);
            foreach (var httpContent in this)
            {
                if (httpContent is ICustomHttpContentConvertable conversable)
                {
                    customHttpContent.Add(conversable.ToCustomHttpContext());
                }
                else
                {
                    customHttpContent.Add(httpContent);
                }
            }
            return customHttpContent;
        }
    }
}
