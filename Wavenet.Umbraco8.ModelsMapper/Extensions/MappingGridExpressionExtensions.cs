// <copyright file="MappingGridExpressionExtensions.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Linq.Expressions;
    using System.Web;

    using Umbraco.Core;
    using Umbraco.Core.Composing;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web;

    using Wavenet.Umbraco8.ModelsMapper.Helpers;

    /// <summary>
    /// Extension class for <see cref="MappingExpression{TDocumentType, TPublishedElement}"/> for grid implementations.
    /// </summary>
    public static class MappingGridExpressionExtensions
    {
        /// <summary>
        /// Gets the view renderer.
        /// </summary>
        /// <value>
        /// The view renderer.
        /// </value>
        private static IViewRenderer ViewRenderer => Current.Factory.GetInstance<IViewRenderer>();

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForGridMember<TDocumentType, TPublishedElement>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, string>> member, string propertyName)
            where TPublishedElement : IPublishedElement
            => mapping.ForMember(member, c => ViewRenderer.GetGridHtml(c.Value(propertyName)));

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForGridMember<TDocumentType, TPublishedElement, TMember>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, TMember>> member, string propertyName)
            where TPublishedElement : IPublishedElement
        {
            if (!MappingHtmlStringExtensions.IsDefined(typeof(TMember)))
            {
                throw new NotSupportedException($"{typeof(TMember).Name} is not defined as IHtmlString");
            }

            return mapping.ForMember(member, c =>
            {
                var attempt = ViewRenderer.GetGridHtml(c.Value(propertyName)).TryConvertTo<TMember>();
                return attempt.Success ? attempt.Result : default!;
            });
        }

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForGridMember<TDocumentType, TPublishedElement>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, IHtmlString>> member, string propertyName)
            where TPublishedElement : IPublishedElement
            => mapping.ForMember(member, c => new HtmlString(ViewRenderer.GetGridHtml(c.Value(propertyName))));
    }
}