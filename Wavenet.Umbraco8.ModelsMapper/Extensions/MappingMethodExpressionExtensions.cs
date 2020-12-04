// <copyright file="MappingMethodExpressionExtensions.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    /// Extension class for <see cref="MappingExpression{TDocumentType, TPublishedElement}"/> for method implementations.
    /// </summary>
    public static class MappingMethodExpressionExtensions
    {
        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForMember<TDocumentType, TPublishedElement, TMember>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, Func<TMember>>> member, Func<TPublishedElement, TMember> implementation)
            where TPublishedElement : IPublishedElement
            => ForMethodCall(mapping, member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForMember<TDocumentType, TPublishedElement, TMember, TParam>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, Func<TParam, TMember>>> member, Func<TPublishedElement, TParam, TMember> implementation)
            where TPublishedElement : IPublishedElement
            => ForMethodCall(mapping, member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForMember<TDocumentType, TPublishedElement, TMember, T1, T2>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, Func<T1, T2, TMember>>> member, Func<TPublishedElement, T1, T2, TMember> implementation)
            where TPublishedElement : IPublishedElement
            => ForMethodCall(mapping, member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the thrid parameter.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        public static MappingExpression<TDocumentType, TPublishedElement> ForMember<TDocumentType, TPublishedElement, TMember, T1, T2, T3>(this MappingExpression<TDocumentType, TPublishedElement> mapping, Expression<Func<TDocumentType, Func<T1, T2, T3, TMember>>> member, Func<TPublishedElement, T1, T2, T3, TMember> implementation)
            where TPublishedElement : IPublishedElement
            => ForMethodCall(mapping, member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member" />.
        /// </summary>
        /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
        /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>
        /// The current mapping.
        /// </returns>
        /// <exception cref="System.ArgumentException">Member should be a method expression like: i =&gt; i.Method.</exception>
        public static MappingExpression<TDocumentType, TPublishedElement> ForMethodCall<TDocumentType, TPublishedElement>(MappingExpression<TDocumentType, TPublishedElement> mapping, LambdaExpression member, Delegate implementation)
            where TPublishedElement : IPublishedElement
        {
            if (!(member.Body is UnaryExpression expression) ||
                !(expression.Operand is MethodCallExpression call) ||
                !(call.Object is ConstantExpression constantExpression) ||
                !(constantExpression.Value is MethodInfo methodInfo))
            {
                throw new ArgumentException("Member should be a method expression like: i => i.Method");
            }

            mapping.ModelMap.Implementations.Add(methodInfo, implementation);
            return mapping;
        }
    }
}