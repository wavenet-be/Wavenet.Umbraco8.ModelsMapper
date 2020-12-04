// <copyright file="ViewRenderer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Helpers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Umbraco.Web;
    using Umbraco.Web.Routing;

    /// <summary>
    /// <see cref="ViewRenderer"/>.
    /// </summary>
    public class ViewRenderer : IViewRenderer
    {
        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        private readonly ControllerContext context;

        /// <summary>
        /// Indicates if this instance is disposed.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRenderer" /> class.
        /// </summary>
        /// <param name="publishedRouter">The published router.</param>
        /// <param name="umbracoContextAccessor">The umbraco context accessor.</param>
        public ViewRenderer(IPublishedRouter publishedRouter, IUmbracoContextAccessor umbracoContextAccessor)
        {
            var controller = new EmptyController();
            var wrapper = new HttpContextWrapper(HttpContext.Current);
            var routeData = new RouteData
            {
                Values = { { "controller", "empty" } },
            };
            this.context = controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            var context = umbracoContextAccessor.UmbracoContext;
            if (context.PublishedRequest?.PublishedContent == null)
            {
                context.PublishedRequest = publishedRouter.CreateRequest(context);
                context.PublishedRequest.PublishedContent = context.Content.GetAtRoot().FirstOrDefault();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the grid HTML.
        /// </summary>
        /// <param name="gridModel">The grid model.</param>
        /// <returns>
        /// The grid HTML.
        /// </returns>
        public virtual string GetGridHtml(object gridModel)
        {
            var view = ViewEngines.Engines.FindPartialView(this.context, "~/Views/Partials/Grid/Bootstrap3.cshtml").View;
            this.context.Controller.ViewData.Model = gridModel;
            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(this.context, view, this.context.Controller.ViewData, this.context.Controller.TempData, sw);
                view.Render(ctx, sw);
                return sw.ToString();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                this.disposedValue = true;
            }
        }

        private class EmptyController : Controller
        {
        }
    }
}