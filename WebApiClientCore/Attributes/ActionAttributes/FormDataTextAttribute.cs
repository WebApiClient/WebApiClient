﻿using System;
using System.Threading.Tasks;

namespace WebApiClientCore.Attributes
{
    /// <summary>
    /// 表示参数值作为 multipart/form-data 表单的一个文本项
    /// </summary>
    public partial class FormDataTextAttribute : ApiActionAttribute
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        private readonly string name = string.Empty;

        /// <summary>
        /// 字段的值
        /// </summary>
        private readonly string? value;

        /// <summary>
        /// 表示 name 和 value 写入 multipart/form-data 表单
        /// </summary>
        /// <param name="name">字段名称</param>
        /// <param name="value">字段的值</param>
        /// <exception cref="ArgumentNullException"></exception>
        [AttributeCtorUsage(AttributeTargets.Interface | AttributeTargets.Method)]
        public FormDataTextAttribute(string name, object? value)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.value = value?.ToString();
        }

        /// <summary>
        /// 执行前
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        public override Task OnRequestAsync(ApiRequestContext context)
        {
            context.HttpContext.RequestMessage.AddFormDataText(this.name, this.value);
            return Task.CompletedTask;
        }
    }
}
