﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using WebApiClientCore.Internals;

namespace WebApiClientCore.Serialization
{
    /// <summary>
    /// keyValue序列化工具
    /// </summary>
    public static class KeyValueSerializer
    {
        /// <summary>
        /// 默认的序列化选项
        /// </summary>
        private static readonly KeyValueSerializerOptions defaultOptions = new();

        /// <summary>
        /// 序列化对象为键值对
        /// </summary>
        /// <param name="key">对象名称</param>
        /// <param name="obj">对象实例</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
        [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
        public static IList<KeyValue> Serialize(string key, object? obj, KeyValueSerializerOptions? options)
        {
            var kvOptions = options ?? defaultOptions;
            if (obj == null)
            {
                if (kvOptions.IgnoreNullValues == true)
                {
                    return Array.Empty<KeyValue>();
                }
                var keyValue = new KeyValue(key, null);
                return [keyValue];
            }

            var objType = obj.GetType();
            var typeCode = Type.GetTypeCode(objType);

            // 不需要考虑转换器的简单类型
            if (typeCode == TypeCode.String ||
                typeCode == TypeCode.Int32 ||
                typeCode == TypeCode.Decimal ||
                typeCode == TypeCode.Double ||
                typeCode == TypeCode.Single)
            {
                var keyValue = new KeyValue(key, obj.ToString());
                return [keyValue];
            }

            if (obj is IEnumerable<KeyValuePair<string, string>> keyValues)
            {
                // 排除字典类型，字典类型要经过Json序列化
                if (objType.IsInheritFrom<IDictionary>() == false)
                {
                    // key的值不经过PropertyNamingPolicy转换，保持原始值
                    return keyValues.Select(item => (KeyValue)item).ToArray();
                }
            }

            return GetKeyValueList(key, obj, objType, kvOptions);
        }

        /// <summary>
        /// 获取键值对
        /// </summary>
        /// <param name="key">对象名称</param>
        /// <param name="obj">对象实例</param>
        /// <param name="objType">对象类型</param>
        /// <param name="options">选项</param>
        /// <returns></returns>
        [RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
        [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
        private static List<KeyValue> GetKeyValueList(string key, object obj, Type objType, KeyValueSerializerOptions options)
        {
            using var bufferWriter = new RecyclableBufferWriter<byte>();
            var jsonOptions = options.GetJsonSerializerOptions();
            var utf8JsonWriter = Utf8JsonWriterCache.Get(bufferWriter, jsonOptions);
            JsonSerializer.Serialize(utf8JsonWriter, obj, objType, jsonOptions);
            var utf8JsonReader = new Utf8JsonReader(bufferWriter.WrittenSpan, new JsonReaderOptions
            {
                MaxDepth = jsonOptions.MaxDepth,
                CommentHandling = jsonOptions.ReadCommentHandling,
                AllowTrailingCommas = jsonOptions.AllowTrailingCommas,
            });

            return options.KeyNamingStyle == KeyNamingStyle.ShortName
                ? GetShortNameKeyValueList(key, ref utf8JsonReader)
                : GetFullNameKeyValueList(key, ref utf8JsonReader, options);
        }

        /// <summary>
        /// 获取ShortName键值对
        /// </summary>
        /// <param name="key"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static List<KeyValue> GetShortNameKeyValueList(string key, ref Utf8JsonReader reader)
        {
            var list = new List<KeyValue>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        {
                            key = reader.GetString() ?? string.Empty;
                            break;
                        }

                    case JsonTokenType.Null:
                        {
                            list.Add(new KeyValue(key, null));
                            break;
                        }

                    case JsonTokenType.String:
                        {
                            var value = reader.GetString();
                            var keyValue = new KeyValue(key, value);
                            list.Add(keyValue);
                            break;
                        }

                    case JsonTokenType.False:
                    case JsonTokenType.True:
                    case JsonTokenType.Number:
                        {
                            var value = Encoding.UTF8.GetString(reader.ValueSpan);
                            var keyValue = new KeyValue(key, value);
                            list.Add(keyValue);
                            break;
                        }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取FullName键值对
        /// </summary>
        /// <param name="key"></param>
        /// <param name="reader"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static List<KeyValue> GetFullNameKeyValueList(string key, ref Utf8JsonReader reader, KeyNamingOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var list = new List<KeyValue>();
            var withRoot = options.KeyNamingStyle == KeyNamingStyle.FullNameWithRoot;
            ParseJsonElement(key, ref root, list, options, withRoot);
            return list;
        }

        /// <summary>
        /// 解析JsonElement
        /// </summary>
        /// <param name="key"></param>
        /// <param name="element"></param>
        /// <param name="list"></param>
        /// <param name="options"></param>
        /// <param name="withRoot"></param>
        private static void ParseJsonElement(string key, ref JsonElement element, IList<KeyValue> list, KeyNamingOptions options, bool withRoot = true)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Null:
                    list.Add(new KeyValue(key, null));
                    break;

                case JsonValueKind.String:
                    list.Add(new KeyValue(key, element.GetString()));
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Number:
                    list.Add(new KeyValue(key, element.GetRawText()));
                    break;

                case JsonValueKind.Object:
                    foreach (var item in element.EnumerateObject())
                    {
                        var ele = item.Value;
                        var itemKey = withRoot
                            ? $"{key}{options.KeyDelimiter}{item.Name}"
                            : item.Name;

                        ParseJsonElement(itemKey, ref ele, list, options);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        var ele = item;
                        var itemKey = $"{key}{options.KeyArrayIndex(index)}";

                        ParseJsonElement(itemKey, ref ele, list, options);
                        index += 1;
                    }
                    break;
            }
        }

    }
}