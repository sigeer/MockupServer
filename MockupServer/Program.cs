using Microsoft.Extensions.Hosting;
using MockupServer;

var app = WebServerService.Create(args);
app.Run();