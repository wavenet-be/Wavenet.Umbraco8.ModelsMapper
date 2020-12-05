// <copyright file="BaseHtmlString.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Internal
{
    using System.Web;

    /// <summary>
    /// Base class for HTML strings.
    /// </summary>
    /// <seealso cref="IHtmlString" />
    public abstract class BaseHtmlString : IHtmlString
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseHtmlString"/> class.
        /// </summary>
        /// <param name="html">The HTML.</param>
        protected BaseHtmlString(string html)
        {
            this.Html = html;
        }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public virtual string Html { get; }

        /// <inheritdoc />
        public virtual string ToHtmlString()
            => this.Html;

        /// <inheritdoc />
        public override string ToString()
            => this.Html;
    }
}