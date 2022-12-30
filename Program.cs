using ElectronNET.API;

namespace Downloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure webhost.
            builder.WebHost.UseElectron(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

#if DEBUG
            // Compile SASS to CSS.
            builder.Services.AddSassCompiler();
#endif

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            if (HybridSupport.IsElectronActive)
                CreateElectronWindow();

            app.Run();
        }

        /// <summary>
        /// Create a electron window and set it's settings.
        /// </summary>
        static async void CreateElectronWindow()
        {
            int min = 512;
            BrowserWindow window = await Electron.WindowManager.CreateWindowAsync();

            window.RemoveMenu();
            window.SetMinimumSize(min, min);
            window.OnClosed += () => Electron.App.Quit();
        }
    }
}