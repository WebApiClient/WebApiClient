﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace WebApiClientCore.Analyzers.Providers
{
    /// <summary>
    /// 表示非方法声明诊断器
    /// </summary>
    sealed class NotMethodDefendedDiagnosticProvider : HttpApiDiagnosticProvider
    {
        /// <summary>
        /// /// <summary>
        /// 获取诊断描述
        /// </summary>
        /// </summary>
        public override DiagnosticDescriptor Descriptor => Descriptors.NotMethodDefendedDescriptor;

        /// <summary>
        /// 非方法声明诊断器
        /// </summary>
        /// <param name="context">上下文</param>
        public NotMethodDefendedDiagnosticProvider(HttpApiContext context)
            : base(context)
        {
        }

        /// <summary>
        /// 返回所有的报告诊断
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Diagnostic> CreateDiagnostics()
        { 
            foreach (var member in this.Context.Syntax.Members)
            {
                if (member.Kind() != SyntaxKind.MethodDeclaration)
                {
                    var location = member.GetLocation();
                    yield return this.CreateDiagnostic(location);
                }
            }
        }
    }
}
