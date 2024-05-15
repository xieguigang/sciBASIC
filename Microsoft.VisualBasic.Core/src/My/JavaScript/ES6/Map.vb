#Region "Microsoft.VisualBasic::d778ee5d576e999a44c42696c94281f5, Microsoft.VisualBasic.Core\src\My\JavaScript\ES6\Map.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 65
    '    Code Lines: 46
    ' Comment Lines: 6
    '   Blank Lines: 13
    '     File Size: 1.93 KB


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
