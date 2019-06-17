using System;
using Microsoft.CodeAnalysis;

namespace Statiq.Razor
{
    public class CompilationErrorException : Exception
    {
        public string Path { get; }

        public FileLinePositionSpan MappedLineSpan { get; }

        public string ErrorMessage { get; }

        public CompilationErrorException(
            string path,
            FileLinePositionSpan mappedLineSpan,
            string errorMessage)
            : base($"({path} {mappedLineSpan.StartLinePosition.Line}:{mappedLineSpan.StartLinePosition.Character}) {errorMessage}")
        {
            Path = path;
            MappedLineSpan = mappedLineSpan;
            ErrorMessage = errorMessage;
        }
    }
}