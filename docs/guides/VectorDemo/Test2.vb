#Region "Microsoft.VisualBasic::c8f35eac7497559cb38a473c2a7da695, sciBASIC#\docs\guides\VectorDemo\Test2.vb"

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

    '   Total Lines: 64
    '    Code Lines: 45
    ' Comment Lines: 5
    '   Blank Lines: 14
    '     File Size: 1.82 KB


    ' Module Test2
    ' 
    '     Sub: Linq, Main, Vector
    '     Class Foo
    ' 
    '         Properties: str
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language

Module Test2

    Class Foo
        Public Property str As String

        Sub New(s$)
            str = s
        End Sub

        Public Overrides Function ToString() As String
            Return str
        End Function

        Public Overloads Shared Operator Like(str As Foo, s$) As Boolean
            With str
                Return Regex.Match(.str, s).Success
            End With
        End Operator
    End Class

    Sub Main()
        Call Linq()
        Call Vector()
    End Sub

    Sub Linq()
        Dim array = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}

        Dim isNumeric = array.Select(Function(s) s Like "\d+").ToArray
        Dim Allstrings = array.Select(Function(s) s.str).ToArray
        Dim Selects = array _
            .Where(Function(s) s Like "\d+") _
            .Select(Function(s) s.str) _
            .ToArray

        ' and then if we want to update the property value?
        With {"ABC", "CBA", "ZZz"}

            For i As Integer = 0 To .Length - 1
                array(i).str = .ref(i)
            Next
        End With
    End Sub

    Sub Vector()
        ' dynamics shadowing a vector from any .NET collection type
        ' Example from the array object that we declared above
        Dim strings = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}.VectorShadows

        Dim isNumeric = strings Like "\d+"
        Dim Allstrings = strings.str
        Dim Selects = strings(strings Like "\d+").str

        ' and then if we want to update the property value?
        strings.str = {"ABC", "CBA", "ZZz"}
        ' all of the property value was set to "123"
        strings.str = "123"

        Pause()
    End Sub
End Module
