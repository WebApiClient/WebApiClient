﻿using System.Collections.Generic;
using System.Linq;
using WebApiClientCore.Attributes;

namespace WebApiClientCore.Extensions.JsonRpc
{
    /// <summary>
    /// 表示JsonRpc参数
    /// </summary>
    sealed class JsonRpcParameters : List<ApiParameterContext>
    {
        /// <summary>
        /// 转换为 jsonRpc 请求参数
        /// </summary>
        /// <param name="paramsStyle"></param>
        /// <returns></returns>
        public object ToJsonRpcParams(JsonRpcParamsStyle paramsStyle)
        {
            if (paramsStyle == JsonRpcParamsStyle.Array)
            {
                return this.Select(item => item.ParameterValue).ToArray();
            }
            else
            {
                return this.ToDictionary(item => item.ParameterName, item => item.ParameterValue);
            }
        }
    }
}
