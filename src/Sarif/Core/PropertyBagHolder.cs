﻿// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Base class for objects that can hold properties of arbitrary types.
    /// </summary>
    public abstract class PropertyBagHolder : IPropertyBagHolder
    {
        [JsonIgnore]
        public IList<string> PropertyNames
        {
            get { return Properties.Keys.ToList(); }
        }

        /// <summary>
        /// Key/value pairs that provide additional information about the run.
        /// </summary>
        [JsonConverter(typeof(PropertyBagConverter))]
        [JsonProperty("properties", DefaultValueHandling = DefaultValueHandling.Ignore)]
        internal abstract IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        public bool TryGetProperty(string propertyName, out string value)
        {
            if (Properties != null && Properties.Keys.Contains(propertyName))
            {
                value = GetProperty(propertyName);
                return true;
            }

            value = null;
            return false;
        }

        public string GetProperty(string propertyName)
        {
            if (!Properties[propertyName].IsString)
            {
                throw new InvalidOperationException(SdkResources.CallGenericGetProperty);
            }

            string value = Properties[propertyName].SerializedValue;

            // Remove the quotes around the serialized value ("x" => x).
            return value.Substring(1, value.Length - 2);
        }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            if (Properties != null && Properties.Keys.Contains(propertyName))
            {
                value = GetProperty<T>(propertyName);
                return true;
            }

            value = default(T);
            return false;
        }

        public T GetProperty<T>(string propertyName)
        {
            if (typeof(T) == typeof(string))
            {
                throw new InvalidOperationException(SdkResources.CallNonGenericGetProperty);
            }

            return JsonConvert.DeserializeObject<T>(Properties[propertyName].SerializedValue);
        }

        public void SetProperty<T>(string propertyName, T value)
        {
            if (Properties == null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>();
            }

            bool isString = typeof(T) == typeof(string);

            string serializedValue = isString
                ? '"' + value.ToString() + '"'
                : JsonConvert.SerializeObject(value);
             
            Properties[propertyName] = new SerializedPropertyInfo(serializedValue, isString);
        }
    }
}