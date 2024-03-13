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

            //�ŧi�W�[���Ҥ覡�A�ϥ� cookie ����
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                //�n�J���A���n�J�ɷ|�۰ʾɨ�n�J��
                options.LoginPath = new PathString("/Login/Index");
                //�n�X����(�i�H�ٲ�)
                options.LogoutPath = new PathString("/Login/Logout");
                //�n�J���Įɶ�
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

            #region ���Ǥ�����

            //�ҥ� cookie ��h�\��
            app.UseCookiePolicy();
            //�ҥΨ����ѧO
            app.UseAuthentication();
            //�ҥα��v�\��
            app.UseAuthorization();

            #endregion

            app.UseSession(); // �����b.UseEndpoints()���e
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            #region ���o���T��Remote IP

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
