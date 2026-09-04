using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PharmacyAPI.Services;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;
using PharmacyAPI.Utils.DbInitializer;
using PharmacyAPI.JwtFeatures;

namespace PharmacyAPI
{
    public static class DependencyInjection
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IDbInitializer, DbInitializer>();
            services.AddScoped<IRepository<Customer>, Repository<Customer>>();
            services.AddScoped<IRepository<Category>, Repository<Category>>();
            services.AddScoped<IRepository<Product>, Repository<Product>>();
            services.AddScoped<IRepository<ProductBatch>, Repository<ProductBatch>>();
            services.AddScoped<IRepository<SalesInvoice>, Repository<SalesInvoice>>();
            services.AddScoped<IRepository<SalesInvoiceItem>, Repository<SalesInvoiceItem>>();
            services.AddScoped<IRepository<Order>, Repository<Order>>();
            services.AddScoped<IRepository<OrderItem>, Repository<OrderItem>>();
            services.AddScoped<IRepository<Cart>, Repository<Cart>>();
            services.AddScoped<IRepository<CartItem>, Repository<CartItem>>();
            services.AddScoped<IRepository<Notification>, Repository<Notification>>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRepository<Chat>, Repository<Chat>>();
            services.AddScoped<IRepository<ChatMessage>, Repository<ChatMessage>>();
            services.AddScoped<IRepository<ApplicationUserOtp>, Repository<ApplicationUserOtp>>();
            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddScoped<IChatService, ChatService>();
        }
    }
}
