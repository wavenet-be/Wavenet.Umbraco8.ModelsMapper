// <copyright file="MappingExpression.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;

    using Umbraco.Core.Models.PublishedContent;

    /// <summary>
    ///   <see cref="MappingExpression{TDocumentType,TPublishedElement}" /> allows you to define the mapping for a Model.
    /// </summary>
    /// <typeparam name="TDocumentType">The type of the document type.</typeparam>
    /// <typeparam name="TPublishedElement">The type of the published element.</typeparam>
    public class MappingExpression<TDocumentType, TPublishedElement>
        where TPublishedElement : IPublishedElement
    {
        /// <summary>
        /// The model map.
        /// </summary>
        private readonly ModelMap modelMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingExpression{TDocumentType,TPublishedElement}"/> class.
        /// </summary>
        /// <param name="modelMap">The model map.</param>
        public MappingExpression(ModelMap modelMap)
        {
            this.modelMap = modelMap;
        }

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        /// <exception cref="ArgumentException">Member should be a property expression like: i => i.Property.</exception>
        public MappingExpression<TDocumentType, TPublishedElement> ForMember<TMember>(Expression<Func<TDocumentType, TMember>> member, Func<TPublishedElement, TMember> implementation)
        {
            if (!(member.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Member should be a property expression like: i => i.Property");
            }

            this.modelMap.Implementations.Add(memberExpression.Member, implementation);
            return this;
        }

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        public MappingExpression<TDocumentType, TPublishedElement> ForMember<TMember>(Expression<Func<TDocumentType, Func<TMember>>> member, Func<TPublishedElement, TMember> implementation)
            => this.ForMethodCall(member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        public MappingExpression<TDocumentType, TPublishedElement> ForMember<TMember, TParam>(Expression<Func<TDocumentType, Func<TParam, TMember>>> member, Func<TPublishedElement, TParam, TMember> implementation)
            => this.ForMethodCall(member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        public MappingExpression<TDocumentType, TPublishedElement> ForMember<TMember, T1, T2>(Expression<Func<TDocumentType, Func<T1, T2, TMember>>> member, Func<TPublishedElement, T1, T2, TMember> implementation)
            => this.ForMethodCall(member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="T3">The type of the thrid parameter.</typeparam>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        public MappingExpression<TDocumentType, TPublishedElement> ForMember<TMember, T1, T2, T3>(Expression<Func<TDocumentType, Func<T1, T2, T3, TMember>>> member, Func<TPublishedElement, T1, T2, T3, TMember> implementation)
            => this.ForMethodCall(member, implementation);

        /// <summary>
        /// Defines the mapping for the specified <paramref name="member"/>.
        /// </summary>
        /// <param name="member">The member to map.</param>
        /// <param name="implementation">The mapping implementation.</param>
        /// <returns>The current mapping.</returns>
        /// <exception cref="System.ArgumentException">Member should be a method expression like: i => i.Method.</exception>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public MappingExpression<TDocumentType, TPublishedElement> ForMethodCall(LambdaExpression member, Delegate implementation)
        {
            if (!(member.Body is UnaryExpression expression) ||
                !(expression.Operand is MethodCallExpression call) ||
                !(call.Object is ConstantExpression constantExpression) ||
                !(constantExpression.Value is MethodInfo methodInfo))
            {
                throw new ArgumentException("Member should be a method expression like: i => i.Method");
            }

            this.modelMap.Implementations.Add(methodInfo, implementation);
            return this;
        }
    }
}