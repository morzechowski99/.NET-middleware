using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.IO;


namespace middleware.Infrastructure
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ImageMiddleware
    {
        private readonly RequestDelegate _next;

        public ImageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {

            string url = httpContext.Request.Path;
            if (url.ToLower().Contains(".jpg"))
            {
                try
                {
                    Image img = Image.FromFile($"./img{url}");
                    MemoryStream stream = new MemoryStream();
                    Font font = new Font("Cambria", 20, FontStyle.Bold, GraphicsUnit.Pixel);
                    //choose color and transparency
                    Color color = Color.FromArgb(0, 0, 0);
                    //location of the watermark text in the parent image
                    Point pt = new Point(10, 10);
                    SolidBrush brush = new SolidBrush(color);
                    string text = "WATERMARK";
                    Graphics graphics = Graphics.FromImage(img);
                    graphics.DrawString(text, font, brush, pt);
                    graphics.Dispose();
                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    httpContext.Response.ContentType = "image/jpeg";
                    return httpContext.Response.Body.WriteAsync(stream.ToArray(), 0,
                    (int)stream.Length);
                }
                catch(FileNotFoundException e)
                {
                    httpContext.Response.StatusCode = 404;
                    
                    return httpContext.Response.WriteAsync("");
                }

            }
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ImageMiddlewareExtensions
    {
        public static IApplicationBuilder UseImageMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageMiddleware>();
        }
    }
}
