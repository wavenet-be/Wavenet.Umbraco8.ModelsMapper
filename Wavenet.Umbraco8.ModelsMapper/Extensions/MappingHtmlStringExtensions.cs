// <copyright file="MappingHtmlStringExtensions.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using Umbraco.Core;

    using Wavenet.Umbraco8.ModelsMapper.ComponentModel;
    using Wavenet.Umbraco8.ModelsMapper.Extensions;
    using Wavenet.Umbraco8.ModelsMapper.Internal;

    /// <summary>
    /// Extension class for <see cref="ModelMappingCollectionBuilder"/> for HTML String interfaces.
    /// </summary>
    public static class MappingHtmlStringExtensions
    {
        /// <summary>
        /// The defined types.
        /// </summary>
        private static readonly HashSet<Type> DefinedTypes = new HashSet<Type>();

        /// <summary>
        /// Defines an HTML string interface.
        /// </summary>
        /// <typeparam name="THtmlString">The type of the HTML string.</typeparam>
        /// <param name="mapping">The mapping.</param>
        /// <returns>
        /// This builder.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// THtmlString should be an interface.
        /// or
        /// THtmlString may only define one method: \"string ToHtmlString()\".
        /// or
        /// THtmlString may only define one property: \"string Html { get; }\".
        /// </exception>
        public static ModelMappingCollectionBuilder DefineHtmlString<THtmlString>(this ModelMappingCollectionBuilder mapping)
            where THtmlString : class
        {
            var htmlStringInterface = typeof(THtmlString);
            if (DefinedTypes.Contains(htmlStringInterface))
            {
                return mapping;
            }

            if (!htmlStringInterface.IsInterface)
            {
                throw new ArgumentException("THtmlString should be an interface.");
            }

            var stringType = typeof(string);
            if (htmlStringInterface.GetAllMethods().Where(m => !m.IsSpecialName).Any(m => m.Name != nameof(BaseHtmlString.ToHtmlString) || m.ReturnType != stringType || m.GetParameters().Any()))
            {
                throw new ArgumentException("THtmlString may only define one method: \"string ToHtmlString()\".");
            }

            var properties = htmlStringInterface.GetAllProperties();
            if (properties.Any(p => p.Name != nameof(BaseHtmlString.Html) || p.PropertyType != stringType || p.SetMethod != null))
            {
                throw new ArgumentException("THtmlString may only define one property: \"string Html { get; }\".");
            }

            var name = new AssemblyName($"ModelsMapper_{htmlStringInterface.FullName}");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(name.FullName);
            var baseClass = typeof(BaseHtmlString);
            var type = module.DefineType(name.FullName, TypeAttributes.Public, baseClass, new[] { htmlStringInterface });
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { stringType });
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, baseClass.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, binder: null, types: new[] { stringType }, modifiers: null));
            il.Emit(OpCodes.Ret);

            var generatedType = type.CreateType();
            TypeDescriptor.AddAttributes(htmlStringInterface, new TypeConverterAttribute(typeof(HtmlStringConverter)));
            var converter = (HtmlStringConverter)TypeDescriptor.GetConverter(htmlStringInterface);
            converter.Constructor = ReflectionUtilities.EmitConstructor<Func<string, BaseHtmlString>>(generatedType.GetConstructor(new[] { stringType }));
            DefinedTypes.Add(htmlStringInterface);
            return mapping;
        }

        /// <summary>
        /// Determines whether the specified type is defined.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is defined; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefined(Type type) => DefinedTypes.Contains(type);

        private class TestConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                => true;

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                Debugger.Log(1, "info", $"TestConverter: {value}");
                return null!;
            }
        }
    }
}