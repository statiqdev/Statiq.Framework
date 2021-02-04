using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Statiq.Common;

namespace Statiq.Razor
{
    internal class StatiqDocumentPhase : RazorEnginePhaseBase
    {
        private readonly string _baseType;  // null indicates the default StatiqRazorPage<>
        private readonly NamespaceCollection _namespaces;
        private readonly bool _isDocumentModel;

        public StatiqDocumentPhase(string baseType, NamespaceCollection namespaces, bool isDocumentModel)
        {
            _baseType = baseType;
            _namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            _isDocumentModel = isDocumentModel;
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            // Get the class declaration and set the base type
            DocumentIntermediateNode documentNode = codeDocument.GetDocumentIntermediateNode();
            NamespaceDeclarationIntermediateNode namespaceDeclaration =
                documentNode.Children.OfType<NamespaceDeclarationIntermediateNode>().Single();
            ClassDeclarationIntermediateNode classDeclaration =
                namespaceDeclaration.Children.OfType<ClassDeclarationIntermediateNode>().Single();
            classDeclaration.BaseType = GetBaseType(documentNode);

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

        private string GetBaseType(DocumentIntermediateNode documentNode)
        {
            // Get the model type (will return "dynamic" if there's no model directive)
            string modelType = ModelDirective.GetModelType(documentNode);

            // Null indicates the default base page
            if (_baseType is null)
            {
                // Use IDocument as the model type if not otherwise specified so that extensions, etc. work as expected
                if (modelType == "dynamic" && _isDocumentModel)
                {
                    return "Statiq.Razor.StatiqRazorPage<Statiq.Common.IDocument>";
                }

                return $"Statiq.Razor.StatiqRazorPage<{modelType}>";
            }

            // An explicit base page type was specified, so just replace the model generic type if there is one
            return _baseType.Replace("<TModel>", "<" + modelType + ">");
        }
    }
}