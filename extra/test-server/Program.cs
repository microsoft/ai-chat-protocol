using Azure.AI.Chat.SampleService;
using Azure.AI.Chat.SampleService.Services;

// Select which backend chat service to use
//GlobalSettings.backendChatService = BackendChatService.AzureOpenAI;
GlobalSettings.backendChatService = BackendChatService.MaaS;
//GlobalSettings.backendChatService = BackendChatService.Llama2MaaP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<ILlama2MaaPClientProvider, Llama2MaaPClientProvider>();
builder.Services.AddScoped<IMaaSClientProvider, MaaSClientProvider>();
builder.Services.AddScoped<IOpenAIClientProvider, OpenAIClientProvider>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

