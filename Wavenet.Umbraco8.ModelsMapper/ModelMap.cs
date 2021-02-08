﻿// <copyright file="ModelMap.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Core.Models;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web;

    using Wavenet.Umbraco8.ModelsMapper.Extensions;
    using Wavenet.Umbraco8.ModelsMapper.Internal;
    using Wavenet.Umbraco8.ModelsMapper.Models;

    /// <summary>
    /// This class map an Umbraco DocumentType to a .net interface.
    /// </summary>
    public class ModelMap
    {
        /// <summary>
        /// The converters.
        /// </summary>
        private readonly IDictionary<PropertyInfo, FieldInfo> converters = new Dictionary<PropertyInfo, FieldInfo>();

        /// <summary>
        /// For all model maps.
        /// </summary>
        private IDictionary<Type, ModelMap>? forAllModelMaps;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMap" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isForAll">Determines if this map is for all interface that extends the specified <paramref name="type"/>.</param>
        /// <exception cref="NotSupportedException">The type: \"{type.FullName}\" is not an interface.</exception>
        public ModelMap(Type type, bool isForAll)
        {
            this.Type = type;
            this.IsForAll = isForAll;
            if (!type.IsInterface)
            {
                throw new NotSupportedException($"The type: \"{type.FullName}\" is not an interface.");
            }
        }

        /// <summary>
        /// Gets the ctor.
        /// </summary>
        /// <value>
        /// The ctor.
        /// </value>
        public Func<IPublishedElement, IPublishedElement>? Ctor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has missing implementation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has missing implementation; otherwise, <c>false</c>.
        /// </value>
        public bool HasMissingImplementation => this.MissingImplementations.Any() || this.GeneratedType == null || this.Ctor == null;

        /// <summary>
        /// Gets the implementations.
        /// </summary>
        /// <value>
        /// The implementations.
        /// </value>
        public Dictionary<MemberInfo, Delegate> Implementations { get; } = new Dictionary<MemberInfo, Delegate>();

        /// <summary>
        /// Gets a value indicating whether this instance is for all interfaces which extends this type.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is for all interfaces which extends this type; otherwise, <c>false</c>.
        /// </value>
        public bool IsForAll { get; }

        /// <summary>
        /// Gets the missing implementation.
        /// </summary>
        /// <value>
        /// The missing implementations.
        /// </value>
        public HashSet<PropertyInfo> MissingImplementations { get; } = new HashSet<PropertyInfo>();

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the type of the generated.
        /// </summary>
        /// <value>
        /// The type of the generated.
        /// </value>
        protected Type? GeneratedType { get; set; }

        /// <summary>
        /// Builds the specified content type.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="forAllModelMaps">For all model maps.</param>
        public void Build(IContentTypeComposition? contentType, IDictionary<Type, ModelMap> forAllModelMaps)
        {
            if (contentType == null)
            {
                return;
            }

            if (this.GeneratedType != null)
            {
                this.FixMissingImplementations(contentType);
                return;
            }

            this.forAllModelMaps = forAllModelMaps;
            var name = new AssemblyName($"ModelsMapper_{this.Type.FullName}");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(name.FullName);
            var type = this.BuildType(module, contentType);
            this.BuildProperties(type, this.Type.GetAllProperties(), contentType);
            this.BuildMethods(type, this.Type.GetAllMethods());
            this.BuildConstructor(type);
            var proxy = this.GeneratedType = type.CreateType();

            foreach (var implementation in this.Implementations)
            {
                proxy.GetField(GetImplementationFieldName(implementation.Key), BindingFlags.Static | BindingFlags.NonPublic)
                    ?.SetValue(null, implementation.Value);
            }

            foreach (var converter in this.converters.ToList())
            {
                FieldInfo field = proxy.GetField(GetConverterFieldName(converter.Key), BindingFlags.Static | BindingFlags.NonPublic);
                this.converters[converter.Key] = field;
                field.SetValue(
                    obj: null,
                    value: TypeDescriptor.GetProperties(converter.Key.DeclaringType).Find(converter.Key.Name, ignoreCase: false).Converter);
            }

            this.Ctor = ReflectionUtilities.EmitConstructor<Func<IPublishedElement, IPublishedElement>>(declaring: proxy);
        }

        /// <summary>
        /// Gets the name of the converter field.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The name of the converter field.</returns>
        private static string GetConverterFieldName(PropertyInfo propertyInfo) => $"{propertyInfo}<converter>";

        /// <summary>
        /// Gets the name of the implementation field.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The name of the implementation field.</returns>
        private static string GetImplementationFieldName(MemberInfo member) => $"{member}<implementation>";

        private static void ProxyMethodImplementation(TypeBuilder type, MemberInfo memberInfo, Delegate implementation)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            var methodInfo = memberInfo as MethodInfo;
            if (propertyInfo == null && methodInfo == null)
            {
                throw new NotSupportedException("Unknown memberInfo type");
            }

            var getter = type.DefineMethod($"get_{memberInfo.Name}", MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo?.PropertyType ?? methodInfo!.ReturnType, methodInfo?.GetParameters().Select(p => p.ParameterType).ToArray() ?? Type.EmptyTypes);
            var il = getter.GetILGenerator();
            var implementationField = type.DefineField(
                fieldName: GetImplementationFieldName(memberInfo),
                type: implementation.GetType(),
                attributes: FieldAttributes.Private | FieldAttributes.Static);
            il.Emit(OpCodes.Ldsfld, implementationField);
            il.Emit(OpCodes.Dup);
            var exception = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, exception);
            il.Emit(OpCodes.Ldarg_0);
            for (int i = 1, c = implementationField.FieldType.GenericTypeArguments.Length - 1; i < c; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            il.Emit(OpCodes.Callvirt, implementationField.FieldType.GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);
            il.MarkLabel(exception);
            il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
            if (propertyInfo != null)
            {
                type.DefineMethodOverride(getter, propertyInfo.GetMethod);
                type.DefineProperty(memberInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes)
                    .SetGetMethod(getter);
            }
            else
            {
                type.DefineMethodOverride(getter, methodInfo);
            }
        }

        /// <summary>
        /// Builds the constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        private void BuildConstructor(TypeBuilder type)
        {
            var baseConstructor = type.BaseType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).First();
            var constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(IPublishedElement) });
            var il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, baseConstructor);
            il.Emit(OpCodes.Ret);
        }

        private void BuildMethods(TypeBuilder type, MethodInfo[] methodInfos)
        {
            var blacklist = new HashSet<Type> { typeof(IPublishedContent), typeof(IPublishedElement) };
            foreach (var methodInfo in methodInfos.Where(m => !blacklist.Contains(m.DeclaringType) && !m.Attributes.HasFlag(MethodAttributes.SpecialName)))
            {
                if (this.TryGetImplementation(methodInfo, out var implementation))
                {
                    ProxyMethodImplementation(type, methodInfo, implementation);
                }
                else if (type.BaseType.GetMethod(methodInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, methodInfo.GetParameters().Select(p => p.ParameterType).ToArray(), null) == null)
                {
                    throw new NotSupportedException($"Method: {methodInfo.Name} is not implemented.");
                }
            }
        }

        private void BuildProperties(TypeBuilder type, IEnumerable<PropertyInfo> propertyInfos, IContentTypeComposition documentType)
        {
            var umbracoProperties = documentType.CompositionPropertyTypes.ToDictionary(
                keySelector: p => p.Alias,
                elementSelector: p => p.Alias,
                comparer: StringComparer.OrdinalIgnoreCase);
            var iPublishType = documentType.IsElement ? typeof(IPublishedElement) : typeof(IPublishedContent);
            var blacklist = new HashSet<Type> { typeof(IPublishedContent), typeof(IPublishedElement) };

            var properties = TypeDescriptor.GetProperties(this.Type);
            foreach (var propertyInfo in propertyInfos.Where(p => !blacklist.Contains(p.DeclaringType)))
            {
                if (propertyInfo.CanWrite || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Any())
                {
                    throw new NotSupportedException($"Interface {this.Type.FullName} should only have readonly properties.");
                }

                if (this.TryGetImplementation(propertyInfo, out var implementation))
                {
                    ProxyMethodImplementation(type, propertyInfo, implementation);
                }
                else if (umbracoProperties.TryGetValue(propertyInfo.Name, out var umbracoProperty))
                {
                    var getter = type.DefineMethod($"get_{propertyInfo.Name}", MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo.PropertyType, Type.EmptyTypes);
                    var il = getter.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, umbracoProperty);
                    this.CallValueMethod(type, il, propertyInfo);
                    il.Emit(OpCodes.Ret);
                    type.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes)
                        .SetGetMethod(getter);
                    type.DefineMethodOverride(getter, propertyInfo.GetMethod);
                }
                else if (type.BaseType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, propertyInfo.PropertyType, Type.EmptyTypes, null) == null)
                {
                    this.MissingImplementations.Add(propertyInfo);
                    this.EnsuresConverterField(type, propertyInfo);
                    ProxyMethodImplementation(type, propertyInfo, MissingImplementationFactory.GetDefaultImplementation(propertyInfo.PropertyType, iPublishType));
                }
            }
        }

        private TypeBuilder BuildType(ModuleBuilder module, IContentTypeComposition contentType)
        {
            var parent = !contentType.IsElement ? typeof(PublishedContentModelMapper) : typeof(PublishedElementModelMapper);
            var interfaces = new[] { this.Type, typeof(IModelMapper) };
            return module.DefineType("ModelsMapper_" + this.Type.FullName, TypeAttributes.Public, parent, interfaces);
        }

        /// <summary>
        /// Calls the value method.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="il">The IL Generator.</param>
        /// <param name="propertyInfo">The property information.</param>
        private void CallValueMethod(Type type, ILGenerator il, PropertyInfo propertyInfo)
        {
            var converter = this.EnsuresConverterField(type, propertyInfo);
            if (converter != null)
            {
                il.Emit(OpCodes.Ldsfld, converter);
                il.Emit(
                    OpCodes.Call,
                    type.BaseType.GetMethod(
                        name: "Value",
                        bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
                        types: new Type[] { typeof(string), typeof(TypeConverter) },
                        binder: null,
                        modifiers: null).MakeGenericMethod(propertyInfo.PropertyType));
            }
            else if (propertyInfo.PropertyType.TryGetGenericArguments(typeof(IEnumerable<>), out var genericArgs) && (genericArgs[0].IsClass || genericArgs[0].IsInterface))
            {
                il.Emit(
                    OpCodes.Call,
                    type.BaseType.GetMethod(
                        name: "EnumerableValue",
                        bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
                        types: new Type[] { typeof(string) },
                        binder: null,
                        modifiers: null).MakeGenericMethod(propertyInfo.PropertyType, genericArgs[0]));
            }
            else
            {
                il.Emit(
                    OpCodes.Call,
                    type.BaseType.GetMethod(
                        name: "Value",
                        bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic,
                        types: new Type[] { typeof(string) },
                        binder: null,
                        modifiers: null).MakeGenericMethod(propertyInfo.PropertyType));
            }
        }

        /// <summary>
        /// Ensureses the converter field if specified <paramref name="propertyInfo"/> has a <see cref="TypeConverterAttribute"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The <see cref="FieldInfo"/> if specified <paramref name="propertyInfo"/> has a <see cref="TypeConverterAttribute"/>; Otherwise <c>null</c>.</returns>
        private FieldInfo? EnsuresConverterField(Type type, PropertyInfo propertyInfo)
        {
            if (!this.converters.TryGetValue(propertyInfo, out var field) && type is TypeBuilder builder && propertyInfo.GetCustomAttribute<TypeConverterAttribute>() != null)
            {
                this.converters[propertyInfo] = field = builder.DefineField(
                            fieldName: GetConverterFieldName(propertyInfo),
                            type: typeof(TypeConverter),
                            attributes: FieldAttributes.Private | FieldAttributes.Static);
            }

            return field;
        }

        private void FixMissingImplementations(IContentTypeComposition documentType)
        {
            if (!this.MissingImplementations.Any() || this.GeneratedType == null)
            {
                return;
            }

            var iPublishType = documentType.IsElement ? typeof(IPublishedElement) : typeof(IPublishedContent);
            var umbracoProperties = documentType.CompositionPropertyTypes.ToDictionary(
                keySelector: p => p.Alias,
                elementSelector: p => p.Alias,
                comparer: StringComparer.OrdinalIgnoreCase);
            foreach (var property in this.MissingImplementations.ToList())
            {
                if (!umbracoProperties.TryGetValue(property.Name, out var alias))
                {
                    continue;
                }

                this.MissingImplementations.Remove(property);
                var name = GetImplementationFieldName(property);
                var getter = new DynamicMethod(name, property.PropertyType, new[] { iPublishType }, this.GeneratedType);
                var il = getter.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, alias);
                this.CallValueMethod(this.GeneratedType, il, property);
                il.Emit(OpCodes.Ret);
                this.GeneratedType.GetField(name, BindingFlags.Static | BindingFlags.NonPublic)
                    ?.SetValue(null, getter.CreateDelegate(typeof(Func<,>).MakeGenericType(iPublishType, property.PropertyType)));
            }
        }

        /// <summary>
        /// Tries to get the specified <paramref name="member"/> implementation.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="implementation">The implementation.</param>
        /// <returns><c>True</c> if an implementation is found otherwise <c>false</c>.</returns>
        private bool TryGetImplementation(MemberInfo member, [NotNullWhen(true)] out Delegate? implementation)
        {
            if (this.Implementations.TryGetValue(member, out implementation))
            {
                return true;
            }
            else if (this.forAllModelMaps != null &&
                     this.forAllModelMaps.TryGetValue(member.DeclaringType, out var forAllMap) &&
                     forAllMap.Implementations.TryGetValue(member, out implementation))
            {
                this.Implementations[member] = implementation;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}