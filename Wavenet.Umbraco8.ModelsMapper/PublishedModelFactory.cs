// <copyright file="PublishedModelFactory.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Umbraco.Core.Models;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Core.Services;

    /// <summary>
    /// This factory will serve all the mapping defined in the application.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Models.PublishedContent.ILivePublishedModelFactory" />
    /// <seealso cref="IPublishedModelFactory" />
    public class PublishedModelFactory : ILivePublishedModelFactory2
    {
        /// <summary>
        /// The content type service.
        /// </summary>
        private readonly IContentTypeService contentTypeService;

        /// <summary>
        /// The media service.
        /// </summary>
        private readonly IMediaTypeService mediaTypeService;

        /// <summary>
        /// The models.
        /// </summary>
        private readonly ModelMappingCollection models;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedModelFactory" /> class.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="mediaTypeService">The media type service.</param>
        public PublishedModelFactory(ModelMappingCollection models, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService)
        {
            this.models = models;
            this.contentTypeService = contentTypeService;
            this.mediaTypeService = mediaTypeService;

            var forAllModelMaps = models.Values.Where(m => m.IsForAll).ToDictionary(keySelector: m => m.Type);
            foreach (var map in models)
            {
                if (map.Value.IsForAll)
                {
                    continue;
                }

                map.Value.Build((IContentTypeComposition)contentTypeService.Get(map.Key) ?? mediaTypeService.Get(map.Key), forAllModelMaps);
            }
        }

        /// <inheritdoc />
        public bool Enabled => true;

        /// <inheritdoc />
        public object SyncRoot { get; } = new object();

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

        /// <inheritdoc />
        public void Refresh()
        {
        }

        /// <inheritdoc />
        public void Reset()
        {
            var forAllMaps = new Lazy<IDictionary<Type, ModelMap>>(() => this.models.Values.Where(m => m.IsForAll).ToDictionary(keySelector: m => m.Type));
            foreach (var map in this.models)
            {
                if (map.Value.MissingImplementations.Any())
                {
                    map.Value.Build((IContentTypeComposition)this.contentTypeService.Get(map.Key) ?? this.mediaTypeService.Get(map.Key), forAllMaps.Value);
                }
            }
        }
    }
}