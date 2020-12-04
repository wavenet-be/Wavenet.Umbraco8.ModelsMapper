// <copyright file="ModelsMapperComposer.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Composing
{
    extern alias ume;

    using Umbraco.Core;
    using Umbraco.Core.Composing;
    using Umbraco.Web;
    using Umbraco.Web.Editors;
    using Umbraco.Web.PublishedCache.NuCache;
    using Umbraco.Web.WebApi;

    using Wavenet.Umbraco8.ModelsMapper.Configuration;
    using Wavenet.Umbraco8.ModelsMapper.Helpers;

    using Embedded = ume::Umbraco.ModelsBuilder.Embedded;
    using EmbeddedModelsBuilderComposer = ume::Umbraco.ModelsBuilder.Embedded.Compose.ModelsBuilderComposer;
    using IModelsBuilderConfig = ume::Umbraco.ModelsBuilder.Embedded.Configuration.IModelsBuilderConfig;
    using PublishedModelFactory = Wavenet.Umbraco8.ModelsMapper.PublishedModelFactory;

    /// <summary>
    /// Setup ModelsMapper.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.ICoreComposer" />
    [ComposeBefore(typeof(NuCacheComposer))]
    [Disable(typeof(EmbeddedModelsBuilderComposer))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ModelsMapperComposer : ICoreComposer
    {
        /// <inheritdoc />
        public void Compose(Composition composition)
        {
            composition.Dashboards()
                .Remove<Embedded.ModelsBuilderDashboard>();

            composition.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Remove<Embedded.BackOffice.ModelsBuilderDashboardController>();

            composition.WithCollectionBuilder<EditorValidatorCollectionBuilder>()
                .Clear()
                .Add<Embedded.BackOffice.ContentTypeModelValidator>()
                .Add<Embedded.BackOffice.MediaTypeModelValidator>()
                .Add<Embedded.BackOffice.MemberTypeModelValidator>();

            composition.RegisterUnique<IModelsBuilderConfig, ModelsMapperConfig>();
            composition.SetPublishedContentModelFactory<PublishedModelFactory>();
            composition.Register<IViewRenderer, ViewRenderer>(Lifetime.Request);
        }
    }
}