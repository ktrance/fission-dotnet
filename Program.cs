using System;
using System.IO;
using Fission.DotNetCore.Compiler;
using System.Collections.Generic;
using Nancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Nancy.Owin;

namespace Fission.DotNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseKestrel()
               .UseUrls("http://*:8888")
               .UseStartup<Startup>()
               .Build();
            host.Run();
        }


    }
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(x => x.UseNancy());
        }
    }

    public class ExecutorModule : NancyModule
    {
        private const string CODEPATH = "/userfunc/user";
        private static Function _userFunc;

        public ExecutorModule()
        {
            Post("/specialize", args => Specialize());
            Get("/", args => Run());
            Post("/", args => Run());
            Put("/", args => Run());
            Head("/", args => Run());
            Options("/", args => Run());
            Delete("/", args => Run());

        }

        private object Specialize()
        {
            var errors = new List<string>();
            if (File.Exists(CODEPATH))
            {
                var code = File.ReadAllText(CODEPATH);
                _userFunc = FissionCompiler.Compile(code, out errors);
                if (_userFunc == null)
                {
                    var errstr = string.Join(Environment.NewLine, errors);
                    var response = (Response)errstr;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;

                }
                return null;
            }
            else { 
                var response = (Response)"Unable to locate code";
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
        }

        private object Run()
        {
            if (_userFunc == null)
            {
                var response = (Response)"Generic container: no requests supported";
                response.StatusCode = HttpStatusCode.InternalServerError;
                return response;
            }
            return _userFunc.Invoke();
        }
    }
}
