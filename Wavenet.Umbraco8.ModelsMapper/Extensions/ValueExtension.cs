// <copyright file="ValueExtension.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Extensions
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Umbraco.Core;
    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    /// <see cref="ValueExtension"/> is an extension helper to simplify the builtin Value extension method.
    /// </summary>
    public static class ValueExtension
    {
        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias.</returns>
        public static TResult EnumerableValue<TResult, TItem>(this IPublishedContent content, string propertyAlias)
            where TResult : IEnumerable<TItem>
        {
            var result = Umbraco.Web.PublishedContentExtensions.Value(content, propertyAlias);
            var attempt = result.TryConvertTo<TResult>();
            if (!attempt.Success && result is IEnumerable enumerable)
            {
                attempt = enumerable.OfType<TItem>().TryConvertTo<TResult>();
            }

            return attempt.Success ? attempt.Result : default!;
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="element">The element.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias.</returns>
        public static TResult EnumerableValue<TResult, TItem>(this IPublishedElement element, string propertyAlias)
            where TResult : IEnumerable<TItem>
        {
            var result = Umbraco.Web.PublishedElementExtensions.Value(element, propertyAlias);
            var attempt = result.TryConvertTo<TResult>();
            if (!attempt.Success && result is IEnumerable enumerable)
            {
                attempt = enumerable.OfType<TItem>().TryConvertTo<TResult>();
            }

            return attempt.Success ? attempt.Result : default!;
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="content">The content.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias.</returns>
        public static TResult Value<TResult>(this IPublishedContent content, string propertyAlias)
            => Umbraco.Web.PublishedContentExtensions.Value<TResult>(content, propertyAlias);

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="element">The element.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The value of the content's property identified by the alias.</returns>
        public static TResult Value<TResult>(this IPublishedElement element, string propertyAlias)
            => Umbraco.Web.PublishedElementExtensions.Value<TResult>(element, propertyAlias);
    }
}