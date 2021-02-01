using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Statiq.Common;

namespace Statiq.Razor
{
    internal class StatiqDocumentPhase : RazorEnginePhaseBase
    {
        private readonly string _baseType; // null indicates the default StatiqRazorPage<>
        private readonly NamespaceCollection _namespaces;
        private readonly object _model;

        public StatiqDocumentPhase(string baseType, NamespaceCollection namespaces, object model)
        {
            _baseType = baseType;
            _namespaces = namespaces;
            _model = model;
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            DocumentIntermediateNode documentNode = codeDocument.GetDocumentIntermediateNode();

            NamespaceDeclarationIntermediateNode namespaceDeclaration =
                documentNode.Children.OfType<NamespaceDeclarationIntermediateNode>().Single();

            // Get the model type (will return "dynamic" if there's no model directive)
            string modelType = ModelDirective.GetModelType(documentNode);

            // Set the base page type and perform default model type substitution here
            ClassDeclarationIntermediateNode classDeclaration =
                namespaceDeclaration.Children.OfType<ClassDeclarationIntermediateNode>().Single();
            if (_baseType is null)
            {
                // Default base page
                if (modelType == "dynamic" && _model is IDocument)
                {
                    // Use IDocument as the model type if not otherwise specified so that extensions, etc. work as expected
                    // Note that nameof does not include the generic type argument so we have to add it to the string
                    classDeclaration.BaseType = "Statiq.Razor.StatiqRazorPage<IDocument>";
                }
                else
                {
                    classDeclaration.BaseType = $"Statiq.Razor.StatiqRazorPage<{modelType}>";
                }
            }
            else
            {
                // Replace the model generic type if there is one
                classDeclaration.BaseType = _baseType.Replace("<TModel>", "<" + modelType + ">");
            }

            // Add namespaces
            int insertIndex = namespaceDeclaration.Children.IndexOf(
                namespaceDeclaration.Children.OfType<UsingDirectiveIntermediateNode>().First());
            foreach (string ns in _namespaces)
            {
                namespaceDeclaration.Children.Insert(
                    insertIndex,
                    new UsingDirectiveIntermediateNode()
                    {
                        Content = ns
                    });
            }

            codeDocument.SetDocumentIntermediateNode(documentNode);
        }
    }
}