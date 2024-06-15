﻿namespace WebApiClientCore.Extensions.OAuths.TokenProviders
{
    /// <summary>
    /// 表示用于刷新 token 的身份信息
    /// </summary>
    public class RefreshTokenCredentials : Credentials
    {
        /// <summary>
        /// 刷新 token值
        /// </summary>
        public string? Refresh_token { get; set; }

        /// <summary>
        /// 扩展信息
        /// 简单模型或字典
        /// </summary>
        public object? Extra { get; set; }
    }
}
