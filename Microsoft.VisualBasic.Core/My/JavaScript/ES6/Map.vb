#Region "Microsoft.VisualBasic::d778ee5d576e999a44c42696c94281f5, Microsoft.VisualBasic.Core\My\JavaScript\ES6\Map.vb"

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

    '     Class RegExp
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: exec, ParseOptions
    ' 
    '     Class Map
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: [set]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection

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

        ''' <summary>
        ''' Parse regexp attribute flags string
        ''' </summary>
        ''' <param name="flags"></param>
        ''' <returns></returns>
        Public Shared Function ParseOptions(flags As String) As RegexOptions
            Dim opts As Index(Of Char) = flags
            Dim int As RegexOptions

            If "g"c Like opts Then
                ' no global
            End If
            If "i"c Like opts Then
                int = int Or RegexOptions.IgnoreCase
            End If
            If "m"c Like opts Then
                int = int Or RegexOptions.Multiline
            End If

            Return int
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
