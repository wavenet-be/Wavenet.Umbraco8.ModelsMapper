// <copyright file="BaseModelsMappingComposer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Composing
{
    using Umbraco.Core.Composing;

    /// <summary>
    /// Base class for document type mapping.
    /// </summary>
    /// <seealso cref="IUserComposer" />
    public abstract class BaseModelsMappingComposer : IUserComposer
    {
        /// <inheritdoc />
        public virtual void Compose(Composition composition)
        {
            this.Map(composition.WithCollectionBuilder<ModelMappingCollectionBuilder>());
        }

        /// <summary>
        /// Maps document types.
        /// </summary>
        /// <param name="mapping">The model mapping collection builder.</param>
        public abstract void Map(ModelMappingCollectionBuilder mapping);
    }
}