﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApiClientCore.Serialization
{
    /// <summary>
    /// 表示KeyValue序列化选项
    /// </summary>
    public sealed class KeyValueSerializerOptions : KeyNamingOptions
    {
        /// <summary>
        /// 包装的 jsonOptions
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// 获取转换器
        /// </summary>
        public IList<JsonConverter> Converters
        {
            get => jsonOptions.Converters;
        }

        /// <summary>
        /// 获取或设置字典键的命名策略
        /// </summary>
        public JsonNamingPolicy? DictionaryKeyPolicy
        {
            get => jsonOptions.DictionaryKeyPolicy;
            set => jsonOptions.DictionaryKeyPolicy = value;
        }

        /// <summary>
        /// 获取或设置是否忽略 null 值
        /// </summary>
        public bool IgnoreNullValues
        {
#if NET5_0_OR_GREATER
            get => jsonOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull;
            set => jsonOptions.DefaultIgnoreCondition = value ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never;
#else
            get => jsonOptions.IgnoreNullValues;
            set => jsonOptions.IgnoreNullValues = value;
#endif
        }

        /// <summary>
        /// 获取或设置是否忽略只读属性
        /// </summary>
        public bool IgnoreReadOnlyProperties
        {
            get => jsonOptions.IgnoreReadOnlyProperties;
            set => jsonOptions.IgnoreReadOnlyProperties = value;
        }

        /// <summary>
        /// 获取或设置最大嵌套层数
        /// 默认值为64
        /// </summary>
        public int MaxDepth
        {
            get => jsonOptions.MaxDepth;
            set => jsonOptions.MaxDepth = value;
        }

        /// <summary>
        /// 获取或设置属性的命名策略
        /// </summary>
        public JsonNamingPolicy? PropertyNamingPolicy
        {
            get => jsonOptions.PropertyNamingPolicy;
            set => jsonOptions.PropertyNamingPolicy = value;
        }

        /// <summary>
        /// KeyValue序列化选项
        /// </summary>
        public KeyValueSerializerOptions()
        {
            this.jsonOptions = new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        /// <summary>
        /// 返回类型的转换器
        /// </summary>
        /// <param name="typeToConvert">目标类型</param>
        /// <returns></returns>
        [RequiresDynamicCode("Getting a converter for a type may require reflection which depends on runtime code generation.")]
        [RequiresUnreferencedCode("Getting a converter for a type may require reflection which depends on unreferenced code.")]
        public JsonConverter GetConverter(Type typeToConvert)
        {
            return this.jsonOptions.GetConverter(typeToConvert);
        }

        /// <summary>
        /// 返回包装的JsonSerializerOptions对象
        /// </summary>
        /// <returns></returns>
        public JsonSerializerOptions GetJsonSerializerOptions()
        {
            return this.jsonOptions;
        }
    }
}
