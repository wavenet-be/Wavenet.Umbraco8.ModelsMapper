// <copyright file="PublishedModelFactory.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    /// This factory will serve all the mapping defined in the application.
    /// </summary>
    /// <seealso cref="IPublishedModelFactory" />
    public class PublishedModelFactory : IPublishedModelFactory
    {
        /// <summary>
        /// The models.
        /// </summary>
        private readonly ModelMappingCollection models;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedModelFactory"/> class.
        /// </summary>
        /// <param name="models">The models.</param>
        public PublishedModelFactory(ModelMappingCollection models)
        {
            this.models = models;
        }

        /// <inheritdoc />
        public IPublishedElement CreateModel(IPublishedElement element)
        {
            if (this.models.TryGetValue(element.ContentType.Alias, out var item) && item.Ctor != null)
            {
                return item.Ctor(element);
            }

            return element;
        }

        /// <inheritdoc />
        public IList CreateModelList(string alias)
            => new List<IPublishedElement>();

        /// <inheritdoc />
        public Type MapModelType(Type type)
            => type;
    }
}