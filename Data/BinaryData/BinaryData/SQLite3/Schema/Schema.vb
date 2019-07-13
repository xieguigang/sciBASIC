#Region "Microsoft.VisualBasic::845d488f090be062278cafc067f90c28, Data\BinaryData\BinaryData\SQLite3\Schema\Schema.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Schema
    ' 
    '         Properties: columns
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseColumns, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ManagedSqlite.Core

    Public Class Schema

        Public Property columns As NamedValue(Of String)()

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(columns$(), removeNameEscape As Boolean)
            Me.columns = ParseColumns(columns, removeNameEscape).ToArray
        End Sub

        Private Iterator Function ParseColumns(columns As String(), removeNameEscape As Boolean) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As String()
            Dim field As NamedValue(Of String)
            Dim name As String
            Dim [nameOf] = Function(text As String())
                               If removeNameEscape Then
                                   Return text(Scan0).GetStackValue("[", "]")
                               Else
                                   Return text(Scan0)
                               End If
                           End Function

            For Each column As String In columns
                tokens = column.StringSplit("\s+")
                name = [nameOf](tokens)
                field = New NamedValue(Of String) With {
                    .Name = name,
                    .Value = tokens(1)
                }

                Yield field
            Next
        End Function

        Public Overrides Function ToString() As String
            Return columns.Keys.GetJson
        End Function
    End Class
End Namespace
