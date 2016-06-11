Imports System.Collections.Generic
Imports System.Text

Namespace YAML.Grammar
    Public Interface ParserInput(Of T)
        ReadOnly Property Length() As Integer

        Function HasInput(pos As Integer) As Boolean

        Function GetInputSymbol(pos As Integer) As T

        Function GetSubSection(position As Integer, length As Integer) As T()

        Function FormErrorMessage(position As Integer, message As String) As String
    End Interface
End Namespace
