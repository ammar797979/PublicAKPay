using AKPay.Models;
using AKPay.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AKPay;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var builder = Host.CreateApplicationBuilder();

        //     builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        //     builder.Services.AddDbContext<AkpayDbContext>(options =>
        //     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            

            // The content between the *s is added to run without db connection, in a normal
            // situation, the above commented code block would be used instead and the following removed

            /************************************************************************************/

            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json.example", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show(
                    "Missing database configuration.\n\n" +
                    "Create src/AKPay/appsettings.json with a ConnectionStrings:DefaultConnection entry " +
                    "(see appsettings.json.example).\n\n" +
                    "Then re-run the app.",
                    "AKPay configuration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            builder.Services.AddDbContext<AkpayDbContext>(options => options.UseSqlServer(connectionString));

            /************************************************************************************/



            // Register both implementations; the selector decides which one is resolved
            builder.Services.AddScoped<AKPayLINQ>();
            builder.Services.AddScoped<AKPaySP>();
            builder.Services.AddScoped<IAKPayService>(sp =>
            {
                return ServiceMode.Current == ServiceFlavor.Sproc
                    ? sp.GetRequiredService<AKPaySP>()
                    : sp.GetRequiredService<AKPayLINQ>();
            });

            // Register forms for DI
            builder.Services.AddTransient<LoginForm>();
            builder.Services.AddTransient<PreAuthForm>();
            builder.Services.AddTransient<RegisterForm>();
            builder.Services.AddTransient<DashboardForm>();
            builder.Services.AddTransient<TopUpForm>();
            builder.Services.AddTransient<U2UTransactionForm>();
            builder.Services.AddTransient<RegularTransactionForm>();
            builder.Services.AddTransient<TransactionHistoryForm>();

            using var host = builder.Build();
            HostAccessor.Current = host;
            Application.Run(new ModeSelectionForm(host));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "AKPay failed to start",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

internal static class HostAccessor
{
    public static IHost? Current { get; set; }
}