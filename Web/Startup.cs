using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RayITUtilityCore31;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Web.Helpers;
using Web.Models;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddXmlSerializerFormatters();

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddDbContext<Netcast2Mp4Context>(options => 
                options.UseSqlServer(Common.AESDecrypt(Configuration.GetConnectionString("DefaultConnection").ToString(), Common.AES_KEY, Common.AES_IV)));

            //宣告增加驗證方式，使用 cookie 驗證
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                //登入頁，未登入時會自動導到登入頁
                options.LoginPath = new PathString("/Login/Index");
                //登出網頁(可以省略)
                options.LogoutPath = new PathString("/Login/Logout");
                //登入有效時間
                options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.Name = "Netcast";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.MaxAge = TimeSpan.FromMinutes(20);
            });

            services.Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => false;
                    options.MinimumSameSitePolicy = SameSiteMode.Strict;
                    options.HttpOnly = HttpOnlyPolicy.Always;
                    options.Secure = CookieSecurePolicy.Always;
                    options.ConsentCookie.Expiration = TimeSpan.FromMinutes(20);
                });

            services.AddSession(options =>
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.Name = "Netcast";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.MaxAge = TimeSpan.FromMinutes(20);
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            #region 次序不能對調

            //啟用 cookie 原則功能
            app.UseCookiePolicy();
            //啟用身分識別
            app.UseAuthentication();
            //啟用授權功能
            app.UseAuthorization();

            #endregion

            app.UseSession(); // 必須在.UseEndpoints()之前
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            #region 取得正確的Remote IP

            //https://ithelp.ithome.com.tw/articles/10199374
            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            // its loopback by default
            forwardingOptions.KnownNetworks.Clear();
            forwardingOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardingOptions);

            #endregion

            WebHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        }
    }
}
