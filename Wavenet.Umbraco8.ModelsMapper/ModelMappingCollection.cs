﻿// <copyright file="ModelMappingCollection.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Umbraco.Core.Composing;
    using Umbraco.Core.Services;

    /// <summary>
    /// The collection of model mappings.
    /// </summary>
    /// <seealso cref="ReadOnlyDictionary{String,ModelMap}" />
    /// <seealso cref="IBuilderCollection{ModelMap}" />
    public class ModelMappingCollection : ReadOnlyDictionary<string, ModelMap>, IBuilderCollection<ModelMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMappingCollection" /> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="mediaTypeService">The media type service.</param>
        public ModelMappingCollection(IDictionary<string, ModelMap> list, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService)
            : base(list)
        {
        }

        /// <inheritdoc />
        IEnumerator<ModelMap> IEnumerable<ModelMap>.GetEnumerator()
            => this.Values.OfType<ModelMap>().GetEnumerator();
    }
}