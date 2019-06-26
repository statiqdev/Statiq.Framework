using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Statiq.Common.Meta.TypeConversion
{
    /// <summary>
    /// Defines a 'split by seperator process' for converting a string represenation of a list of values.
    /// </summary>
    /// <remarks>
    /// Originally based on code from UniversalTypeConverter by Thorsten Bruning
    /// Licensed under MS-PL
    /// https://www.codeproject.com/articles/248440/universal-type-converter
    /// </remarks>
    internal class GenericStringSplitter : IStringSplitter
    {
        private readonly string _seperator;

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringSplitter">GenericStringSplitter</see> class using the <see cref="F:TB.ComponentModel.UniversalTypeConverter.DefaultStringSeperator">semicolon</see> as seperator.
        /// </summary>
        public GenericStringSplitter()
          : this(";")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GenericStringSplitter">GenericStringSplitter</see> class.
        /// </summary>
        /// <param name="seperator">The seperator to use for splitting.</param>
        public GenericStringSplitter(string seperator)
        {
            _seperator = seperator ?? throw new ArgumentNullException(nameof(seperator));
        }

        /// <summary>
        /// Splits the given string represenation of a list of values.
        /// </summary>
        /// <param name="valueList">String represenation of the list to split.</param>
        /// <returns>A list of the splitted values.</returns>
        public string[] Split(string valueList)
        {
            return valueList.Split(
                new string[1]
                {
                    _seperator
                },
                StringSplitOptions.None);
        }
    }
}