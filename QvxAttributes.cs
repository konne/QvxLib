/*
    This Library is to have an easy access to Qvx Files and the Qlikview
    Connector Interface.
  
    Copyright (C) 2011  Konrad Mattheis (mattheis@ukma.de)
 
    This Software is available under the GPL and a comercial licence.
    For further information to the comercial licence please contact
    Konrad Mattheis. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

namespace QvxLib
{
    #region Usings
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    #endregion

    #region QvxBaseAttribute
    public abstract class QvxBaseAttribute : Attribute
    {
    }
    #endregion

    #region QvxIgnoreAttribute
    [AttributeUsage(AttributeTargets.All)]
    public class QvxIgnoreAttribute : QvxBaseAttribute
    {
        #region Variables & Properties
        private bool ignore = false;

        public static readonly QvxIgnoreAttribute Default = new QvxIgnoreAttribute(false);

        public static readonly QvxIgnoreAttribute No = new QvxIgnoreAttribute(false);

        public static readonly QvxIgnoreAttribute Yes = new QvxIgnoreAttribute(true);

        public bool Ignore
        {
            get
            {
                return ignore;
            }
        }
        #endregion

        #region Constructor
        public QvxIgnoreAttribute(bool ignore)
        {
            this.ignore = ignore;
        } 
        #endregion

        #region IsDefaultAttribute
        public override bool IsDefaultAttribute()
        {
            return this.Ignore == Default.Ignore;
        } 
        #endregion
    }
    #endregion

    #region QvxSubClassAsStringAttribute
    [AttributeUsage(AttributeTargets.All)]
    public class QvxSubClassAsStringAttribute : QvxBaseAttribute
    {
        #region Variables & Properties
        public static readonly QvxSubClassAsStringAttribute Default = new QvxSubClassAsStringAttribute(false);

        public static readonly QvxSubClassAsStringAttribute No = new QvxSubClassAsStringAttribute(false);

        public static readonly QvxSubClassAsStringAttribute Yes = new QvxSubClassAsStringAttribute(true);

        private bool subClassAsString = false;
        public bool SubClassAsString
        {
            get
            {
                return subClassAsString;
            }
        } 
        #endregion

        #region Constructor
        public QvxSubClassAsStringAttribute(bool subClassAsString)
        {
            this.subClassAsString = subClassAsString;
        } 
        #endregion

        #region IsDefaultAttribute
        public override bool IsDefaultAttribute()
        {
            return this.SubClassAsString == Default.SubClassAsString;
        } 
        #endregion
    }
    #endregion

    #region QvxMaxLengthAttribute
    [AttributeUsage(AttributeTargets.All)]
    public class QvxMaxLengthAttribute : QvxBaseAttribute
    {
        private int maxLength = 0;

        public QvxMaxLengthAttribute(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public int MaxLength
        {
            get
            {
                return maxLength;
            }
        }
    }
    #endregion

    #region IQvxFieldAttribute
    /// <summary>
    /// Interface for QvxFieldAttributes, to handle an Easy apply of the
    /// Attributes to the FieldAttributes
    /// </summary>
    public interface IQvxFieldAttribute
    {
        void ApplyAttribute(FieldAttributes fieldAttributes);
    }
    #endregion

    #region Extend class FieldAttrbutes with ApplyAttribute from Attributes
    public partial class FieldAttributes
    {
        public void ApplyCustomAttributes(IEnumerable<Attribute> Attributes)
        {
            foreach (var item in Attributes)
            {
                var FieldAttribute = item as IQvxFieldAttribute;
                if (FieldAttribute != null)
                    FieldAttribute.ApplyAttribute(this);
            }
        }
    }
    #endregion

    #region QvxFieldAttributeTypeAttribute
    /// <summary>
    /// Attribute to apply a Data type <see cref="FieldAttrType"/> Enum.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributesOverrideTypeAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        /// <summary>
        /// Gets the type.
        /// </summary>
        public FieldAttrType Type { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="QvxFieldAttributesOverrideTypeAttribute"/> class.
        /// </summary>
        /// <param name="Type">The Field Type <see cref="FieldAttrType"/> Enum.</param>
        public QvxFieldAttributesOverrideTypeAttribute(FieldAttrType Type)
        {
            this.Type = Type;
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.Type = this.Type;
        }
        #endregion
    }
    #endregion

    #region QvxFieldAttributenDecAttribute
    /// <summary>
    /// Attribute that specifices the Fixed number for decimals for FIX type data and fixed number 
    /// of significant digits for REAL type data. Allowed range(0..15).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributenDecAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        public int nDec { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QvxFieldAttributenDecAttribute"/> class.
        /// </summary>
        /// <param name="nDec">The nDec Parameter. Allowed range(0..15)</param>
        public QvxFieldAttributenDecAttribute(int nDec)
        {
            if (nDec >= 0 && nDec <= 15)
                this.nDec = nDec;
            else
                throw new ArgumentException("Allowed range(0..15)");
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.nDec = this.nDec;
            fieldAttributes.nDecSpecified = true;
        }
        #endregion
    }
    #endregion

    #region QvxFieldAttributeuseThouAttribute
    /// <summary>
    /// Attribute that set a Flag that indicating if thousand separator is used. Allowed range(0,1).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributeuseThouAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        /// <summary>
        /// Gets the use thou.
        /// </summary>
        public int useThou { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QvxFieldAttributeuseThouAttribute"/> class.
        /// </summary>
        /// <param name="useThou">The use thousand separator Allowed range(0,1)</param>
        public QvxFieldAttributeuseThouAttribute(int useThou)
        {
            if (useThou == 0 || useThou == 1)
                this.useThou = useThou;
            else
                throw new ArgumentException("Allowed range(0,1)");
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.UseThou = this.useThou;
            fieldAttributes.UseThouSpecified = true;
        }
        #endregion
    }
    #endregion

    #region QvxFieldAttributeFmtAttribute
    /// <summary>
    /// Attribute to apply a special Format code.
    /// Format code that can be used to specify format for numbers, dates, time, timestamps and time intervals
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributeFmtAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        /// <summary>
        /// Gets the FMT.
        /// </summary>
        public string fmt { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QvxFieldAttributeFmtAttribute"/> class.
        /// </summary>
        /// <param name="fmt">The FMT.</param>
        public QvxFieldAttributeFmtAttribute(string fmt)
        {
            this.fmt = fmt;
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.Fmt = this.fmt;
        }
        #endregion
    }
    #endregion

    #region QvxFieldAttributeDecAttribute
    /// <summary>
    /// Attribute to apply a Decimal separator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributeDecAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        /// <summary>
        /// Gets the dec.
        /// </summary>
        public string Dec { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QvxFieldAttributeDecAttribute"/> class.
        /// </summary>
        /// <param name="Dec">The dec.</param>
        public QvxFieldAttributeDecAttribute(string Dec)
        {
            this.Dec = Dec;
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.Dec = this.Dec;
        }
        #endregion
    }
    #endregion

    #region QvxFieldAttributeThouAttribute
    /// <summary>
    /// Attribute to apply a Thousand separator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class QvxFieldAttributeThouAttribute : QvxBaseAttribute, IQvxFieldAttribute
    {
        #region Variables
        /// <summary>
        /// Gets the Thou.
        /// </summary>
        public string Thou { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="QvxFieldAttributeThouAttribute"/> class.
        /// </summary>
        /// <param name="Dec">The Thou.</param>
        public QvxFieldAttributeThouAttribute(string Thou)
        {
            this.Thou = Thou;
        }
        #endregion

        #region ApplyAttribute
        /// <summary>
        /// Applies the Attribute.
        /// </summary>
        /// <param name="fieldAttributes">The field Attributes.</param>
        public void ApplyAttribute(FieldAttributes fieldAttributes)
        {
            fieldAttributes.Thou = this.Thou;
        }
        #endregion
    }
    #endregion
}
