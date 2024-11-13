﻿// -----------------------------------------------------------------------
// <copyright file="PrefabHelper.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.API.Features
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    using Exiled.API.Enums;
    using Exiled.API.Features.Attributes;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// Helper for Prefabs.
    /// </summary>
    public static class PrefabHelper
    {
        private static readonly Dictionary<PrefabType, GameObject> Stored = new();

        /// <summary>
        /// Gets a dictionary of <see cref="PrefabType"/> to <see cref="GameObject"/>.
        /// </summary>
        public static ReadOnlyDictionary<PrefabType, GameObject> PrefabToGameObject => new(Stored);

        /// <summary>
        /// Gets the prefab attribute of a prefab type.
        /// </summary>
        /// <param name="prefabType">The prefab type.</param>
        /// <returns>The <see cref="PrefabAttribute" />.</returns>
        public static PrefabAttribute GetPrefabAttribute(this PrefabType prefabType)
        {
            var type = prefabType.GetType();
            return type.GetField(Enum.GetName(type, prefabType)).GetCustomAttribute<PrefabAttribute>();
        }

        /// <summary>
        /// Gets the prefab of the specified <see cref="PrefabType"/>.
        /// </summary>
        /// <param name="type">The <see cref="PrefabType"/> to get prefab of.</param>
        /// <typeparam name="T">The <see cref="Component"/> to get.</typeparam>
        /// <returns>Returns the prefab component as {T}.</returns>
        public static T GetPrefab<T>(PrefabType type)
            where T : Component
        {
            if (!Stored.TryGetValue(type, out var gameObject) || !gameObject.TryGetComponent(out T component))
                return null;

            return component;
        }

        /// <summary>
        /// Spawns a prefab on server.
        /// </summary>
        /// <param name="prefabType">The prefab type.</param>
        /// <param name="position">The position to spawn the prefab.</param>
        /// <param name="rotation">The rotation of the prefab.</param>
        /// <returns>The <see cref="GameObject"/> instantied.</returns>
        public static GameObject Spawn(PrefabType prefabType, Vector3 position = default, Quaternion rotation = default)
        {
            if (!Stored.TryGetValue(prefabType, out var gameObject))
                return null;
            var newGameObject = UnityEngine.Object.Instantiate(gameObject, position, rotation);
            NetworkServer.Spawn(newGameObject);
            return newGameObject;
        }

        /// <summary>
        /// Spawns a prefab on server.
        /// </summary>
        /// <param name="prefabType">The prefab type.</param>
        /// <param name="position">The position to spawn the prefab.</param>
        /// <param name="rotation">The rotation of the prefab.</param>
        /// <typeparam name="T">The <see cref="Component"/> type.</typeparam>
        /// <returns>The <see cref="Component"/> instantied.</returns>
        public static T Spawn<T>(PrefabType prefabType, Vector3 position = default, Quaternion rotation = default)
            where T : Component
        {
            var obj = UnityEngine.Object.Instantiate(GetPrefab<T>(prefabType), position, rotation);
            NetworkServer.Spawn(obj.gameObject);
            return obj;
        }

        /// <summary>
        /// Loads all prefabs.
        /// </summary>
        internal static void LoadPrefabs()
        {
            Stored.Clear();

            foreach (var prefabType in EnumUtils<PrefabType>.Values)
            {
                var attribute = prefabType.GetPrefabAttribute();
                Stored.Add(prefabType, NetworkClient.prefabs.FirstOrDefault(prefab => prefab.Key == attribute.AssetId || prefab.Value.name.Contains(attribute.Name)).Value);
            }
        }
    }
}