using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ValidatorCollection : IValidatorCollection
    {
        private readonly List<IValidator> _validators = new List<IValidator>();

        private readonly Engine _engine;

        internal ValidatorCollection(Engine engine)
        {
            _engine = engine;
        }

        public void Add(IValidator validator)
        {
            if (validator is object)
            {
                _validators.Add(validator);
            }
        }

        internal async Task ValidateAsync(PipelinePhase pipelinePhase)
        {
            // Run validators
            ValidationContext validationContext = new ValidationContext(_engine, pipelinePhase);
            await _validators
                .Where(v => v.Phases?.Contains(pipelinePhase.Phase) != false && v.Pipelines?.Contains(pipelinePhase.PipelineName, StringComparer.OrdinalIgnoreCase) != false)
                .ParallelForEachAsync(async v => await v.ValidateAsync(validationContext));

            // Log results
            bool fail = false;
            foreach (ValidationResult result in validationContext.Results)
            {
                string documentPart = result.Document is object
                    ? $" [{result.Document.ToSafeDisplayString()}]"
                    : string.Empty;
                _engine.Logger.Log(
                    result.LogLevel,
                    $"Validation{documentPart}: {result.Message}");
                if (result.LogLevel >= (_engine.Settings.GetBool(Keys.FailValidationOnWarnings) ? LogLevel.Warning : LogLevel.Error))
                {
                    fail = true;
                }
            }

            // Throw if validation failed
            if (fail)
            {
                throw new Exception($"Validation for pipeline {pipelinePhase.PipelineName}/{pipelinePhase.Phase} failed");
            }
        }
    }
}
