// <copyright file="ReflectionExtensions.cs" company="Wavenet">
// Copyright (c) Wavenet. All rights reserved.
// </copyright>

namespace Wavenet.Umbraco8.ModelsMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Contains method helper for reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets all methods the specified <paramref name="type"/> has which incudes methods defined in ancestors.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>All methods the specified <paramref name="type"/> has.</returns>
        public static MethodInfo[] GetAllMethods(this Type type)
        {
            if (type.IsInterface)
            {
                var methods = new List<MethodInfo>();
                var knwonTypes = new HashSet<Type>();
                var queue = new Queue<Type>();
                knwonTypes.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    type = queue.Dequeue();
                    Type[] interfaces = type.GetInterfaces();
                    foreach (Type item in interfaces)
                    {
                        if (!knwonTypes.Contains(item))
                        {
                            knwonTypes.Add(item);
                            queue.Enqueue(item);
                        }
                    }

                    methods.InsertRange(
                        0,
                        type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                             .Where(m => !methods.Contains(m)));
                }

                return methods.ToArray();
            }
            else
            {
                return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            }
        }
    }
}