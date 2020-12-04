// <copyright file="IViewRenderer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Helpers
{
    using System;

    /// <summary>
    /// <see cref="IViewRenderer"/> allows you to render an Umbraco grid without the view context.
    /// </summary>
    public interface IViewRenderer : IDisposable
    {
        /// <summary>
        /// Gets the grid HTML.
        /// </summary>
        /// <param name="gridModel">The grid model.</param>
        /// <returns>
        /// The grid HTML.
        /// </returns>
        string GetGridHtml(object gridModel);
    }
}