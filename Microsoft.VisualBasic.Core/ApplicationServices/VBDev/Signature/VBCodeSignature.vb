Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace ApplicationServices.Development

    Module VBCodeSignature

        Const AccessPattern$ = "((Public)|(Private)|(Friend)|(Protected)|(\s))*"
        Const TypePatterns$ = "^\s+" & AccessPattern & "\s*((Class)|(Module)|(Structure)|(Enum))\s+" & VBLanguage.IdentiferPattern
        Const PropertyPatterns$ = "^\s+" & AccessPattern & "\s*((ReadOnly)|(WriteOnly)|(Default)|(\s))*\s*Property\s+" & VBLanguage.IdentiferPattern
        Const MethodPatterns$ = "^\s+" & AccessPattern & "\s*((Sub)|(Function)|(Iterator)|(\s))*\s+" & VBLanguage.IdentiferPattern

        <Extension>
        Public Iterator Function PopulateModules(vb As String) As IEnumerable(Of NamedCollection(Of String))



        End Function
    End Module
End Namespace