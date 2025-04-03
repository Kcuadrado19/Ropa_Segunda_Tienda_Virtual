using System;
using System.Reflection;

namespace Ropa_Segunda_Tienda_Virtual.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}