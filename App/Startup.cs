using App.Clients;
using App.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace App
{
    /// <summary>
    /// ����ҳ
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// ��ȡ����
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// ����
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>     
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }


        /// <summary>
        /// ��ӷ���
        /// </summary>
        /// <param name="services"></param>  
        public void ConfigureServices(IServiceCollection services)
        {
            // ��ӿ�����
            services.AddControllers().AddXmlSerializerFormatters();

            // Ӧ�ñ���ʱ���ɽӿڵĴ������ʹ���
            services
                .AddWebApiClient()
                .UseJsonFirstApiActionDescriptor()
                .UseSourceGeneratorHttpApiActivator();

            // ע��userApi
            services.AddHttpApi(typeof(IUserApi), o =>
            {
                o.UseLogging = Environment.IsDevelopment();
                o.HttpHost = new Uri("http://localhost:5000/");
            });

            // ע��������clientIdģʽ��token����ѡ��
            services.AddClientCredentialsTokenProvider<IUserApi>(o =>
            {
                o.Endpoint = new Uri("http://localhost:5000/api/tokens");
                o.Credentials.Client_id = "clientId";
                o.Credentials.Client_secret = "xxyyzz";
            });

            // userApi�ͻ��˺�̨����
            services.AddScoped<UserService>().AddHostedService<UserHostedService>();
            services.AddDynamicHostSupport();
        }

        /// <summary>
        /// �����м��
        /// </summary>
        /// <param name="app"></param>    
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
