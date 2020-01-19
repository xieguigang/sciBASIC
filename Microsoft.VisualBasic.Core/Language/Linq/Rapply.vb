Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Language

    ''' <summary>
    ''' Helper for implements ``lapply`` and ``sapply`` liked operations from R language
    ''' </summary>
    Public Module Rapply

        <Extension>
        Public Function lapply(Of Tin As INamedValue, TOut)(sequence As IEnumerable(Of Tin), apply As [Delegate], ParamArray args As Object()) As Dictionary(Of String, TOut)

        End Function
    End Module
End Namespace
