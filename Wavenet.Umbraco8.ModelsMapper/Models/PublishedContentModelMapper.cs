﻿// <copyright file="PublishedContentModelMapper.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Core.Models.PublishedContent;

    using WebPublishedContentExtensions = Umbraco.Web.PublishedContentExtensions;

    /// <summary>
    /// Base class for strongly-typed mapping of a published content.
    /// </summary>
    /// <seealso cref="PublishedContentWrapped" />
    public abstract class PublishedContentModelMapper : PublishedContentWrapped
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentModelMapper" /> class.
        /// </summary>
        /// <param name="content">The original content.</param>
        protected PublishedContentModelMapper(IPublishedContent content)
            : base(content)
        {
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>
        /// The value of the content's property identified by the alias.
        /// </returns>
        protected TResult EnumerableValue<TResult, TItem>(string propertyAlias)
            where TResult : IEnumerable<TItem>
        {
            var result = WebPublishedContentExtensions.Value(this, propertyAlias);
            if (result == null)
            {
                result = Enumerable.Empty<TItem>();
            }

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
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>
        /// The value of the content's property identified by the alias.
        /// </returns>
        protected TResult Value<TResult>(string propertyAlias)
        {
            return WebPublishedContentExtensions.Value<TResult>(this, propertyAlias);
        }

        /// <summary>
        /// Gets the value of a content's property identified by its alias.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="converter">The converter.</param>
        /// <returns>
        /// The value of the content's property identified by the alias.
        /// </returns>
        protected TResult Value<TResult>(string propertyAlias, TypeConverter converter)
        {
            var value = WebPublishedContentExtensions.Value(this, propertyAlias);
            if (GlobalSettings.DebugMode && value != null && !converter.CanConvertFrom(value.GetType()))
            {
                throw new NotSupportedException($"The converter: '{converter.GetType().Name}' cannot convert '{value.GetType().Name}'");
            }

            return (TResult)converter.ConvertFrom(value);
        }
    }
}