Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ApplicationServices.Development

    Module VBCodeSignature

        Const TypePatterns$ = "^\s+(Class)|(Module)|(Structure)|(Enum)\s+" & Text.

        <Extension>
        Public Iterator Function PopulateModules(vb As String) As IEnumerable(Of NamedCollection(Of String))



        End Function
    End Module
End Namespace