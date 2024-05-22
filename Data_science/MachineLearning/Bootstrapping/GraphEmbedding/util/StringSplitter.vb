#Region "Microsoft.VisualBasic::901d2f500ed3228ef260d0dfb9e71576, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\util\StringSplitter.vb"

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

    '   Total Lines: 36
    '    Code Lines: 30 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (16.67%)
    '     File Size: 1.38 KB


    '     Class StringSplitter
    ' 
    '         Function: RemoveEmptyEntries, split
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Namespace GraphEmbedding.util

    Public Class StringSplitter
        Private Shared ReadOnly r As Regex = New Regex("(\(x,y\))|(\(y,x\))|(\(y,z\))|(\(x,z\))|(\(z,x\))")

        Public Shared Function split(separator As String, original As String) As String()
            Dim separator_char As Char() = separator.ToCharArray()
            For i = 1 To separator_char.Length - 1
                original = original.Replace(separator_char(i), separator_char(0))
            Next
            original = r.Replace(original, "")
            Return original.Split(CChar(separator.Substring(0, 1)))
        End Function

        Public Shared Function RemoveEmptyEntries(original As String()) As String()
            Dim len = original.Length
            Dim list As IList(Of String) = New List(Of String)(len)
            For i = 0 To len - 1
                If Not ReferenceEquals(original(i), Nothing) AndAlso Not original(i).Equals("") Then
                    list.Add(original(i))
                End If
            Next
            Dim result = New String(list.Count - 1) {}
            For i = 0 To list.Count - 1
                result(i) = list(i).Trim()
            Next
            Return result
        End Function

    End Class

End Namespace
