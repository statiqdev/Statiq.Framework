using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing.Validation
{
    public class TestValidatorCollection : IValidatorCollection
    {
        public List<IValidator> Validators { get; } = new List<IValidator>();

        public void Add(IValidator validator) => Validators.Add(validator);
    }
}
