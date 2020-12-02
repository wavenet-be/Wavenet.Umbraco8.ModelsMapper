// <copyright file="ModelMap.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web;

    using Wavenet.Umbraco8.ModelsMapper.Extensions;
    using Wavenet.Umbraco8.ModelsMapper.Internal;

    /// <summary>
    /// This class map an Umbraco DocumentType to a dot net interface.
    /// </summary>
    public class ModelMap
    {
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
        public void Build(IContentTypeBase contentType, IDictionary<Type, ModelMap> forAllModelMaps)
        {
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

            this.Ctor = ReflectionUtilities.EmitConstructor<Func<IPublishedElement, IPublishedElement>>(declaring: proxy);
        }

        /// <summary>
        /// Gets the name of the implementation field.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The name of the implementation field.</returns>
        private static string GetImplementationFieldName(MemberInfo member) => $"{member}<implementation>";

        /// <summary>
        /// Gets the property types of the specified <paramref name="documentType"/>.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <returns>The property types of the specified <paramref name="documentType"/>.</returns>
        private static IEnumerable<PropertyType> GetPropertyTypes(IContentTypeBase documentType)
        {
            return documentType is IContentTypeComposition composition ?
                composition.CompositionPropertyTypes :
                documentType.PropertyTypes;
        }

        /// <summary>
        /// Gets the value method for .
        /// </summary>
        /// <param name="iPublishType">Type of the i publish.</param>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>
        /// The appropriate Value method for the specified <paramref name="propertyInfo" />.
        /// </returns>
        private static MethodInfo GetValueMethod(Type iPublishType, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.TryGetGenericArguments(typeof(IEnumerable<>), out var genericArgs) && (genericArgs[0].IsClass || genericArgs[0].IsInterface))
            {
                return typeof(ValueExtension).GetMethod("EnumerableValue", new Type[] { iPublishType, typeof(string) }).MakeGenericMethod(propertyInfo.PropertyType, genericArgs[0]);
            }
            else
            {
                return typeof(ValueExtension).GetMethod("Value", new Type[] { iPublishType, typeof(string) }).MakeGenericMethod(propertyInfo.PropertyType);
            }
        }

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
            il.Emit(OpCodes.Ldarg_0);
            for (int i = 1, c = implementationField.FieldType.GenericTypeArguments.Length - 1; i < c; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            il.Emit(OpCodes.Callvirt, implementationField.FieldType.GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);
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
                    throw new InvalidOperationException($"Method: {methodInfo.Name} is not implemented.");
                }
            }
        }

        private void BuildProperties(TypeBuilder type, IEnumerable<PropertyInfo> propertyInfos, IContentTypeBase documentType)
        {
            var umbracoProperties = GetPropertyTypes(documentType).ToDictionary(
                keySelector: p => p.Alias,
                comparer: StringComparer.OrdinalIgnoreCase);
            var iPublishType = documentType.IsElement ? typeof(IPublishedElement) : typeof(IPublishedContent);
            var blacklist = new HashSet<Type> { typeof(IPublishedContent), typeof(IPublishedElement) };

            foreach (var propertyInfo in propertyInfos.Where(p => !blacklist.Contains(p.DeclaringType)))
            {
                if (propertyInfo.CanWrite || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0)
                {
                    throw new InvalidOperationException($"Interface {this.Type.FullName} should only have readonly properties.");
                }

                if (this.TryGetImplementation(propertyInfo, out var implementation))
                {
                    ProxyMethodImplementation(type, propertyInfo, implementation);
                }
                else if (umbracoProperties.TryGetValue(propertyInfo.Name, out var umbracoProperty))
                {
                    var getter = type.DefineMethod($"get_{propertyInfo.Name}", MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyInfo.PropertyType, Type.EmptyTypes);
                    var il = getter.GetILGenerator();
                    var value = GetValueMethod(iPublishType, propertyInfo);
                    Type fallbackType = typeof(Fallback);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, umbracoProperty.Alias);
                    il.Emit(OpCodes.Call, value);
                    il.Emit(OpCodes.Ret);
                    type.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, Type.EmptyTypes)
                        .SetGetMethod(getter);
                    type.DefineMethodOverride(getter, propertyInfo.GetMethod);
                }
                else if (type.BaseType.GetProperty(propertyInfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, propertyInfo.PropertyType, Type.EmptyTypes, null) == null)
                {
                    throw new InvalidOperationException($"Property not found: {propertyInfo.Name} in document type {documentType.Alias}.");
                }
            }
        }

        private TypeBuilder BuildType(ModuleBuilder module, IContentTypeBase contentType)
        {
            var parent = !contentType.IsElement ? typeof(PublishedContentModel) : typeof(PublishedElementModel);
            var interfaces = new[] { this.Type, typeof(IModelMapper) };
            return module.DefineType(this.Type.FullName, TypeAttributes.Public, parent, interfaces);
        }

        /// <summary>
        /// Tries to get the specified <paramref name="member"/> implementation.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="implementation">The implementation.</param>
        /// <returns><c>True</c> if an implementation is found otherwise <c>false</c>.</returns>
        private bool TryGetImplementation(MemberInfo member, out Delegate implementation)
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