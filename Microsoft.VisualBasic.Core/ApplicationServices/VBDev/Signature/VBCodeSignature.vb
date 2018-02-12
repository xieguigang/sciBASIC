Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.SymbolBuilder

Namespace ApplicationServices.Development

    Module VBCodeSignature

        Const AccessPattern$ = "((Public)|(Private)|(Friend)|(Protected)|(\s))*"
        Const TypePatterns$ = "^\s+" & AccessPattern & "\s*((Class)|(Module)|(Structure)|(Enum)|(Delegate))\s+" & VBLanguage.IdentiferPattern
        Const PropertyPatterns$ = "^\s+" & AccessPattern & "\s*((ReadOnly)|(WriteOnly)|(Default)|(\s))*\s*Property\s+" & VBLanguage.IdentiferPattern
        Const MethodPatterns$ = "^\s+" & AccessPattern & "\s*((Sub)|(Function)|(Iterator)|(\s))*\s+" & VBLanguage.IdentiferPattern

        <Extension>
        Public Iterator Function PopulateModules(vb As String) As IEnumerable(Of NamedCollection(Of String))
            Dim types$() = vb.Matches(TypePatterns, RegexICMul).ToArray
            Dim properties$() = vb.Matches(PropertyPatterns, RegexICMul).ToArray
            Dim methods$() = vb.Matches(MethodPatterns, RegexICMul).ToArray

            ' 使用InStr进行位置的确定
            ' 并且使用前导的空格的数量来确定从属关系


        End Function
    End Module
End Namespace