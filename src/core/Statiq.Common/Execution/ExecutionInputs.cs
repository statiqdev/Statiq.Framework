using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Immutable;

namespace Statiq.Common
{
    /// <summary>
    /// A special query type that exposes the underlying immutable array.
    /// </summary>
    /// <remarks>
    /// Intended for use by the infrastructure, users are not expected to
    /// create new instances of this class.
    /// </remarks>
    public class ExecutionInputs : Query<IDocument>, IReadOnlyList<IDocument>
    {
        public ImmutableArray<IDocument> Array { get; }

        public int Count => ((IReadOnlyList<IDocument>)Array).Count;

        public IDocument this[int index] => ((IReadOnlyList<IDocument>)Array)[index];

        public ExecutionInputs(ImmutableArray<IDocument> inputs, IExecutionContext context)
            : base(inputs, context)
        {
            Array = inputs;
        }

        public static implicit operator ImmutableArray<IDocument>(ExecutionInputs inputs) => inputs.Array;
    }
}
