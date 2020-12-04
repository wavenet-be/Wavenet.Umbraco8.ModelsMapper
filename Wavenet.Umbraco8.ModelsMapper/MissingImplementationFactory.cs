// <copyright file="MissingImplementationFactory.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    /// This factory create a <see cref="Func{IPublishedElement,TResult}"/> for a property without Umbraco or mapping information.
    /// </summary>
    public static class MissingImplementationFactory
    {
        /// <summary>
        /// The implementation.
        /// </summary>
        private static readonly Dictionary<(Type, Type), Delegate> Implementation = new Dictionary<(Type, Type), Delegate>();

        /// <summary>
        /// Gets the default implementation.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="publishedType">Type of the published.</param>
        /// <returns>
        /// The default implementation.
        /// </returns>
        public static Delegate GetDefaultImplementation(Type returnType, Type publishedType)
        {
            if (!Implementation.TryGetValue((returnType, publishedType), out var implementation))
            {
                Implementation[(returnType, publishedType)] = implementation = typeof(MissingImplementationFactory)
                    .GetMethod(nameof(EmptyImplementation), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(returnType)
                    .CreateDelegate(typeof(Func<,>).MakeGenericType(publishedType, returnType));
            }

            return implementation;
        }

#nullable disable warnings

        /// <summary>
        /// Empty implementation.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="element">The element.</param>
        /// <returns>The default value of <typeparamref name="TResult"/>.</returns>
        private static TResult EmptyImplementation<TResult>(IPublishedElement element) => default(TResult);

#nullable restore warnings
    }
}