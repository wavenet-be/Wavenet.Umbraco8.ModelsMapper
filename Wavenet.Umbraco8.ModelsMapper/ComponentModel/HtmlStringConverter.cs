// <copyright file="HtmlStringConverter.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;

    using Wavenet.Umbraco8.ModelsMapper.Internal;

    /// <summary>
    /// HtmlString type converter.
    /// </summary>
    /// <seealso cref="TypeConverter" />
    public class HtmlStringConverter : TypeConverter
    {
        /// <summary>
        /// Gets or sets the constructor.
        /// </summary>
        /// <value>
        /// The constructor.
        /// </value>
        public Func<string, BaseHtmlString>? Constructor { get; set; }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || typeof(IHtmlString).IsAssignableFrom(sourceType);

        /// <inheritdoc />
        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => this.Constructor?.Invoke(value?.ToString() ?? string.Empty);
    }
}