// <copyright file="MappingExpression.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Linq.Expressions;

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
        /// Initializes a new instance of the <see cref="MappingExpression{TDocumentType,TPublishedElement}"/> class.
        /// </summary>
        /// <param name="modelMap">The model map.</param>
        public MappingExpression(ModelMap modelMap)
        {
            this.ModelMap = modelMap;
        }

        /// <summary>
        /// Gets the model map.
        /// </summary>
        /// <value>
        /// The model map.
        /// </value>
        public ModelMap ModelMap { get; }

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

            if (this.ModelMap.IsForAll && this.ModelMap.Type != memberExpression.Member.DeclaringType)
            {
                throw new ArgumentException($"{memberExpression.Member.Name} should be part of {this.ModelMap.Type.Name} (in for all mode).");
            }

            this.ModelMap.Implementations.Add(memberExpression.Member, implementation);
            return this;
        }
    }
}