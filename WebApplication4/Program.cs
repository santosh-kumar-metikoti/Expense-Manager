using Mjml.AspNetCore;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Configuration;
using WebApplication4.Schedulers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<QuartzHostedService>();
builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
// Add our job
builder.Services.AddSingleton<RemindersJob>();
builder.Services.AddSingleton(new JobSchedule(
    jobType: typeof(RemindersJob),
cronExpression: "0 34 2 ? * WED *"));

/*builder.Services.AddMjmlServices(o => {
    o.DefaultKeepComments = true;
    o.DefaultBeautify = true;
});*/


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
app.Run();
