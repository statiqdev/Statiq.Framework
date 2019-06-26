using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Statiq.Common.Meta.TypeConversion
{
    /// <summary>
    /// The exception that is thrown when a conversion is invalid.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning
    /// Licensed under MS-PL
    /// https://www.codeproject.com/articles/248440/universal-type-converter
    /// </remarks>
    public class InvalidConversionException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConversionException">InvalidConversionException</see> class.
        /// </summary>
        public InvalidConversionException(object valueToConvert, Type destinationType)
          : base(string.Format("'{0}' ({1}) is not convertible to '{2}'.", valueToConvert, valueToConvert == null ? (object)(Type)null : (object)valueToConvert.GetType(), (object)destinationType))
        {
        }
    }
}
