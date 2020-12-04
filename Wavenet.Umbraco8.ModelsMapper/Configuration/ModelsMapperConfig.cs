// <copyright file="ModelsMapperConfig.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Configuration
{
    extern alias ume;

    using ume::Umbraco.ModelsBuilder.Embedded.Configuration;

    /// <summary>
    /// Models mapper has all its configuration hardcoded.
    /// This class is only there to activate built-in functionnality required by the library.
    /// </summary>
    /// <seealso cref="IModelsBuilderConfig" />
    public class ModelsMapperConfig : IModelsBuilderConfig
    {
        /// <inheritdoc />
        public bool AcceptUnsafeModelsDirectory => false;

        /// <inheritdoc />
        public int DebugLevel => 0;

        /// <inheritdoc />
        public bool Enable => true;

        /// <inheritdoc />
        public bool EnableFactory => false;

        /// <inheritdoc />
        public bool FlagOutOfDateModels => false;

        /// <inheritdoc />
        public bool IsDebug => false;

        /// <inheritdoc />
        public string? ModelsDirectory => null;

        /// <inheritdoc />
        public ModelsMode ModelsMode => ModelsMode.Nothing;

        /// <inheritdoc />
        public string? ModelsNamespace => null;
    }
}