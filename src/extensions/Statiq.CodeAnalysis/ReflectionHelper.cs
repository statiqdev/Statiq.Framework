using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Statiq.CodeAnalysis
{
    // From https://gist.github.com/jamietre/4476307
    // See also https://stackoverflow.com/a/13318056
    public static class ReflectionHelper
    {
        public static string[] GetCallSignatures(Type type, string callThrough)
        {
            Type[] types = type.IsInterface
                ? type.GetInterfaces().Concat(new[] { type }).ToArray()
                : new[] { type };
            return types.SelectMany(t => t.GetMethods()
                    .Where(m => !m.IsSpecialName && !m.DeclaringType.Equals(typeof(object)))
                    .Select(m => $"{GetMethodSignature(m)} => {callThrough}.{GetMethodSignature(m, true)};"))
                .Concat(types.SelectMany(t => t.GetProperties()
                    .Where(m => !m.IsSpecialName && !m.DeclaringType.Equals(typeof(object)))
                    .Select(p => $"public {TypeName(p.PropertyType)} {PropertyName(p, false)} => {callThrough}{PropertyName(p, true)};")))
                .ToArray();
        }

        private static string PropertyName(PropertyInfo propertyInfo, bool callable)
        {
            ParameterInfo[] parameters = propertyInfo.GetIndexParameters();
            if (parameters.Length > 0)
            {
                // Indexer
                return $"{(callable ? string.Empty : "this")}[{BuildParameterSignature(parameters, null, callable)}]";
            }
            return $"{(callable ? "." : string.Empty)}{propertyInfo.Name}";
        }

        /// <summary>
        /// Return the method signature as a string.
        /// </summary>
        /// <param name="method">
        /// The Method.
        /// </param>
        /// <param name="callable">
        /// Return as an callable string(public void a(string b) would return a(b))
        /// </param>
        /// <returns>
        /// Method signature.
        /// </returns>
        public static string GetMethodSignature(MethodInfo method, bool callable = false)
        {
            // Special case to use interface syntax for GetEnumerator() methods
            if (method.Name.Equals(nameof(IEnumerable.GetEnumerator))
                && method.DeclaringType.IsInterface
                && method.GetParameters().Length == 0)
            {
                return callable
                    ? "GetEnumerator()"
                    : $"{TypeName(method.ReturnType)} {TypeName(method.DeclaringType)}.GetEnumerator()";
            }

            StringBuilder sigBuilder = new StringBuilder();

            BuildReturnSignature(sigBuilder, method, callable);

            sigBuilder.Append("(");
            sigBuilder.Append(BuildParameterSignature(method.GetParameters(), method, callable));
            sigBuilder.Append(")");

            // Generic constraints

            foreach (Type arg in method.GetGenericArguments())
            {
                List<string> constraints = new List<string>();
                foreach (Type constraint in arg.GetGenericParameterConstraints())
                {
                    constraints.Add(TypeName(constraint));
                }

                GenericParameterAttributes attrs = arg.GenericParameterAttributes;

                if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    constraints.Add("class");
                }
                if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    constraints.Add("struct");
                }
                if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    constraints.Add("new()");
                }
                if (constraints.Count > 0 && !callable)
                {
                    sigBuilder.Append(" where " + TypeName(arg) + ": " + string.Join(", ", constraints));
                }
            }

            return sigBuilder.ToString();
        }

        private static string BuildParameterSignature(ParameterInfo[] parameters, MethodInfo method, bool callable)
        {
            StringBuilder sigBuilder = new StringBuilder();
            bool firstParam = true;
            bool secondParam = false;
            foreach (ParameterInfo param in parameters)
            {
                if (firstParam)
                {
                    firstParam = false;
                    if (method?.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) == true)
                    {
                        if (callable)
                        {
                            secondParam = true;
                            continue;
                        }
                        sigBuilder.Append("this ");
                    }
                }
                else if (secondParam)
                {
                    secondParam = false;
                }
                else
                {
                    sigBuilder.Append(", ");
                }

                if (param.IsOut)
                {
                    sigBuilder.Append("out ");
                }
                else if (param.ParameterType.IsByRef)
                {
                    sigBuilder.Append("ref ");
                }

                if (!callable && IsParamArray(param))
                {
                    sigBuilder.Append("params ");
                }

                if (!callable)
                {
                    sigBuilder.Append(TypeName(param.ParameterType));
                    sigBuilder.Append(' ');
                }

                sigBuilder.Append(param.Name);

                if (param.IsOptional && !callable)
                {
                    sigBuilder.Append(" = ").Append(
                        param.ParameterType.Equals(typeof(bool))
                            ? param.DefaultValue.ToString().ToLower() // special case for bool since it returns "True"/"False"
                            : (param.DefaultValue ?? "default"));
                }
            }
            return sigBuilder.ToString();
        }

        /// <summary>
        /// Get full type name with full namespace names.
        /// </summary>
        /// <param name="type">
        /// Type. May be generic or nullable.
        /// </param>
        /// <returns>
        /// Full type name, fully qualified namespaces.
        /// </returns>
        public static string TypeName(Type type)
        {
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
            {
                return TypeName(nullableType) + "?";
            }

            if (!type.IsGenericType)
            {
                if (type.IsArray)
                {
                    return TypeName(type.GetElementType()) + "[]";
                }

                string name = type.Name.TrimEnd('&');
                return name switch
                {
                    "String" => "string",
                    "Int16" => "short",
                    "UInt16" => "ushort",
                    "Int32" => "int",
                    "UInt32" => "uint",
                    "Int64" => "long",
                    "UInt64" => "ulong",
                    "Decimal" => "decimal",
                    "Double" => "double",
                    "Object" => "object",
                    "Void" => "void",
                    _ => string.IsNullOrWhiteSpace(type.FullName) ? name : type.FullName.TrimEnd('&'),
                };
            }

            StringBuilder sb = new StringBuilder(type.Name.Substring(0, type.Name.IndexOf('`')));

            sb.Append('<');
            bool first = true;
            foreach (Type t in type.GetGenericArguments())
            {
                if (!first)
                {
                    sb.Append(',');
                }

                sb.Append(TypeName(t));
                first = false;
            }
            sb.Append('>');
            return sb.ToString();
        }

        private static void BuildReturnSignature(StringBuilder sigBuilder, MethodInfo method, bool callable = false)
        {
            bool firstParam = true;
            if (!callable)
            {
                sigBuilder.Append(Visibility(method)).Append(' ');

                if (method.IsStatic)
                {
                    sigBuilder.Append("static ");
                }

                sigBuilder.Append(TypeName(method.ReturnType));
                sigBuilder.Append(' ');
            }
            sigBuilder.Append(method.Name);

            // Add method generics
            if (method.IsGenericMethod)
            {
                sigBuilder.Append("<");
                foreach (Type g in method.GetGenericArguments())
                {
                    if (firstParam)
                    {
                        firstParam = false;
                    }
                    else
                    {
                        sigBuilder.Append(", ");
                    }

                    sigBuilder.Append(TypeName(g));
                }
                sigBuilder.Append(">");
            }
        }

        private static string Visibility(MethodInfo method)
        {
            if (method.IsPublic)
            {
                return "public";
            }
            else if (method.IsPrivate)
            {
                return "private";
            }
            else if (method.IsAssembly)
            {
                return "internal";
            }
            else if (method.IsFamily)
            {
                return "protected";
            }
            else
            {
                throw new Exception("I wasn't able to parse the visibility of this method.");
            }
        }

        private static bool IsParamArray(ParameterInfo info) =>
            info.GetCustomAttribute(typeof(ParamArrayAttribute), true) != null;
    }
}
