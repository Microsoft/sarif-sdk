// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public struct WhatComponent : IEquatable<WhatComponent>
    {
        public string Category { get; }
        public string Location { get; }
        public string PropertySet { get; }
        public string PropertyName { get; }
        public string PropertyValue { get; }

        public WhatComponent(string category, string location, string propertySet, string propertyName, string propertyValue)
        {
            this.Category = category ?? "";
            this.Location = location ?? "";
            this.PropertySet = propertySet ?? "";
            this.PropertyName = propertyName ?? "";
            this.PropertyValue = propertyValue ?? "";
        }

        public override string ToString()
        {
            return $"{Category} | {Location} | {PropertySet} | {PropertyName} | {PropertyValue}";
        }

        public bool Equals(WhatComponent other)
        {
            return string.Equals(this.PropertyValue, other.PropertyValue)
                && string.Equals(this.PropertyName, other.PropertyName)
                && string.Equals(this.PropertySet, other.PropertySet)
                && string.Equals(this.Location, other.Location)
                && string.Equals(this.Category, other.Category);
        }

        public override bool Equals(object obj)
        {
            if (obj is WhatComponent whatComponent)
            {
                return Equals(whatComponent);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;

            unchecked
            {
                if (this.Category != null)
                {
                    hashCode = hashCode * 31 + this.Category.GetHashCode();
                }

                if (this.Location != null)
                {
                    hashCode = hashCode * 31 + this.Location.GetHashCode();
                }

                if (this.PropertySet != null)
                {
                    hashCode = hashCode * 31 + this.PropertySet.GetHashCode();
                }

                if (this.PropertyName != null)
                {
                    hashCode = hashCode * 31 + this.PropertyName.GetHashCode();
                }

                if (this.PropertyValue != null)
                {
                    hashCode = hashCode * 31 + this.PropertyValue.GetHashCode();
                }
            }

            return hashCode;
        }

        public static bool operator ==(WhatComponent left, WhatComponent right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WhatComponent left, WhatComponent right)
        {
            return !(left == right);
        }
    }
}
