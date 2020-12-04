// <copyright file="ModelMappingCollectionBuilder.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections.Generic;

    using Umbraco.Core;
    using Umbraco.Core.Composing;
    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    /// <see cref="ModelMappingCollection"/> Builder.
    /// </summary>
    /// <seealso cref="ICollectionBuilder{ModelMappingCollection, ModelMap}" />
    public class ModelMappingCollectionBuilder : ICollectionBuilder<ModelMappingCollection, ModelMap>
    {
        /// <summary>
        /// The maps.
        /// </summary>
        private Dictionary<string, ModelMap> maps = new Dictionary<string, ModelMap>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        ModelMappingCollection ICollectionBuilder<ModelMappingCollection, ModelMap>.CreateCollection(IFactory factory)
        {
            return factory.CreateInstance<ModelMappingCollection>(this.maps);
        }

        /// <summary>
        /// Defines mapping for the specified <paramref name="documentTypeAlias" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the elemnt type.</typeparam>
        /// <param name="documentTypeAlias">The document type alias.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>This builder.</returns>
        public ModelMappingCollectionBuilder DefineElementMap<TDocumentType>(string documentTypeAlias, Action<MappingExpression<TDocumentType, IPublishedElement>>? configuration = null)
        {
            var map = new ModelMap(typeof(TDocumentType), isForAll: false);
            this.maps.Add(documentTypeAlias, map);
            configuration?.Invoke(new MappingExpression<TDocumentType, IPublishedElement>(map));
            return this;
        }

        /// <summary>
        /// Defines mapping for the specified <paramref name="documentTypeAlias" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <param name="documentTypeAlias">The document type alias.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>This builder.</returns>
        public ModelMappingCollectionBuilder DefineMap<TDocumentType>(string documentTypeAlias, Action<MappingExpression<TDocumentType, IPublishedContent>>? configuration = null)
        {
            var map = new ModelMap(typeof(TDocumentType), isForAll: false);
            this.maps.Add(documentTypeAlias, map);
            configuration?.Invoke(new MappingExpression<TDocumentType, IPublishedContent>(map));
            return this;
        }

        /// <summary>
        /// Defines a base mapping for all document types which extends the specified <typeparamref name="TDocumentType"/>.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <param name="configuration">The configuration.</param>
        /// <returns>This builder.</returns>
        public ModelMappingCollectionBuilder DefineMapForAll<TDocumentType>(Action<MappingExpression<TDocumentType, IPublishedContent>> configuration)
        {
            var map = new ModelMap(typeof(TDocumentType), isForAll: true);
            this.maps.Add($"${typeof(TDocumentType).FullName}$", map);
            configuration(new MappingExpression<TDocumentType, IPublishedContent>(map));
            return this;
        }

        /// <inheritdoc />
        void ICollectionBuilder.RegisterWith(IRegister register)
        {
            ICollectionBuilder<ModelMappingCollection, ModelMap> self = this;
            register.Register(self.CreateCollection, Lifetime.Singleton);
        }
    }
}