#Region "Microsoft.VisualBasic::0a05b3989db587a4921abf47b09e4722, Microsoft.VisualBasic.Core\My\JavaScript\ES6\Map.vb"

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
