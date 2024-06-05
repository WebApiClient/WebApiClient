﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebApiClientCore.Analyzers.SourceGenerator
{
    /// <summary>
    /// HttpApi代理类
    /// </summary>
    sealed class HttpApiProxyClass : IEquatable<HttpApiProxyClass>
    {
        /// <summary>
        /// 接口符号
        /// </summary>
        private readonly INamedTypeSymbol httpApi;
        private readonly string httpApiFullName;

        /// <summary>
        /// 拦截器变量名
        /// </summary>
        private readonly string apiInterceptorFieldName = $"apiInterceptor_{(uint)Environment.TickCount}";

        /// <summary>
        /// action执行器变量名
        /// </summary>
        private readonly string actionInvokersFieldName = $"actionInvokers_{(uint)Environment.TickCount}";

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName => this.httpApi.ToDisplayString();

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => $"WebApiClientCore.{this.httpApi.ContainingNamespace}";

        /// <summary>
        /// 类型名
        /// </summary>
        public string ClassName => this.httpApi.Name;

        /// <summary>
        /// HttpApi代理类
        /// </summary>
        /// <param name="httpApi"></param>
        public HttpApiProxyClass(INamedTypeSymbol httpApi)
        {
            this.httpApi = httpApi;
            this.httpApiFullName = GetFullName(httpApi);
        }

        /// <summary>
        /// 获取完整名称
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private static string GetFullName(ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        /// <summary>
        /// 转换为SourceText
        /// </summary>
        /// <returns></returns>
        public SourceText ToSourceText()
        {
            // System.Diagnostics.Debugger.Launch();
            var code = this.ToString();
            return SourceText.From(code, Encoding.UTF8);
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("#pragma warning disable");
            builder.AppendLine("using System;");
            builder.AppendLine($"namespace {this.Namespace}");
            builder.AppendLine("{");

            builder.AppendLine($"\t[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"\t[global::System.Diagnostics.DebuggerTypeProxy(typeof({this.httpApiFullName}))]");
            builder.AppendLine($"\t[global::WebApiClientCore.HttpApiProxyClass(typeof({this.httpApiFullName}))]");
            builder.AppendLine($"\tpartial class {this.ClassName}:{this.httpApiFullName}");
            builder.AppendLine("\t{");

            builder.AppendLine($"\t\tprivate readonly global::WebApiClientCore.IHttpApiInterceptor {this.apiInterceptorFieldName};");
            builder.AppendLine($"\t\tprivate readonly global::WebApiClientCore.ApiActionInvoker[] {this.actionInvokersFieldName};");

            builder.AppendLine($"\t\tpublic {this.httpApi.Name}(global::WebApiClientCore.IHttpApiInterceptor apiInterceptor, global::WebApiClientCore.ApiActionInvoker[] actionInvokers)");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\tthis.{this.apiInterceptorFieldName} = apiInterceptor;");
            builder.AppendLine($"\t\t\tthis.{this.actionInvokersFieldName} = actionInvokers;");
            builder.AppendLine("\t\t}");
            builder.AppendLine();

            var index = 0;
            foreach (var interfaceType in this.httpApi.AllInterfaces.Append(httpApi))
            {
                foreach (var method in interfaceType.GetMembers().OfType<IMethodSymbol>())
                {
                    var methodCode = this.BuildMethod(interfaceType, method, index);
                    builder.AppendLine(methodCode);
                    index += 1;
                }
            }


            builder.AppendLine("\t}");
            builder.AppendLine("}");
            builder.AppendLine("#pragma warning restore");

            return builder.ToString();
        }

        /// <summary>
        /// 构建方法
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="method"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string BuildMethod(INamedTypeSymbol interfaceType, IMethodSymbol method, int index)
        {
            var builder = new StringBuilder();
            var parametersString = string.Join(",", method.Parameters.Select(item => $"{GetFullName(item.Type)} {item.Name}"));
            var parameterNamesString = string.Join(",", method.Parameters.Select(item => item.Name));
            var paremterArrayString = string.IsNullOrEmpty(parameterNamesString)
                ? "global::System.Array.Empty<global::System.Object>()"
                : $"new global::System.Object[] {{ {parameterNamesString} }}";

            var methodName = $"\"{interfaceType.ToDisplayString()}.{method.Name}\"";
            var returnTypeString = GetFullName(method.ReturnType);
            builder.AppendLine($"\t\t[global::WebApiClientCore.HttpApiProxyMethod({index}, {methodName})]");
            builder.AppendLine($"\t\t{returnTypeString} {GetFullName(interfaceType)}.{method.Name}( {parametersString} )");
            builder.AppendLine("\t\t{");
            builder.AppendLine($"\t\t\treturn ({returnTypeString})this.{this.apiInterceptorFieldName}.Intercept(this.{this.actionInvokersFieldName}[{index}], {paremterArrayString});");
            builder.AppendLine("\t\t}");
            return builder.ToString();
        }

        /// <summary>
        /// 是否与目标相等
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(HttpApiProxyClass other)
        {
            return this.FileName == other.FileName;
        }

        /// <summary>
        /// 是否与目标相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is HttpApiProxyClass builder)
            {
                return this.Equals(builder);
            }
            return false;
        }

        /// <summary>
        /// 获取哈希
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.FileName.GetHashCode();
        }


        /// <summary>
        /// 表示MethodInfo的相等比较器
        /// </summary>
        private class MethodEqualityComparer : IEqualityComparer<IMethodSymbol>
        {
            public static MethodEqualityComparer Default { get; } = new MethodEqualityComparer();

            public bool Equals(IMethodSymbol x, IMethodSymbol y)
            {
                if (x.Name != y.Name || !x.ReturnType.Equals(y.ReturnType, SymbolEqualityComparer.Default))
                {
                    return false;
                }

                var xParameterTypes = x.Parameters.Select(p => p.Type);
                var yParameterTypes = y.Parameters.Select(p => p.Type);
                return xParameterTypes.SequenceEqual(yParameterTypes, SymbolEqualityComparer.Default);
            }

            public int GetHashCode(IMethodSymbol obj)
            {
#pragma warning disable RS1024
                var hashCode = 0;
                hashCode ^= obj.Name.GetHashCode();
                hashCode ^= obj.ReturnType.GetHashCode();
                foreach (var parameter in obj.Parameters)
                {
                    hashCode ^= parameter.Type.GetHashCode();
                }
                return hashCode;
#pragma warning restore RS1024
            }
        }
    }
}
