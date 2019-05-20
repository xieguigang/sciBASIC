Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports System.Text.RegularExpressions


Namespace My.JavaScript.ES6

    Public Class RegExp

        Dim r As Regex

        Sub New(pattern$, Optional flags$ = Nothing)
            If flags.StringEmpty Then
                r = New Regex(pattern)
            Else
                r = New Regex(pattern, ParseOptions(flags))
            End If
        End Sub

        Public Function exec(text As String) As String()
            Return match(text, r)
        End Function

        Public Shared Function ParseOptions(flags As String) As RegexOptions

        End Function
    End Class

    Public Class Map : Inherits Dictionary(Of String, String)

        Sub New()
        End Sub

        Sub New(table As Dictionary(Of String, String))
            Call MyBase.New(table)
        End Sub

        Public Sub [set](key As String, value As String)
            Me(key) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(json As XElement) As Map
            Return New Map(json.Value.LoadJSON(Of Dictionary(Of String, String)))
        End Operator
    End Class
End Namespace