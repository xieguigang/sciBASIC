#Region "Microsoft.VisualBasic::1e9fa74a2d267b278d262dc9efed5633, Data_science\NLP\Extensions.vb"

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

    '   Total Lines: 37
    '    Code Lines: 27 (72.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (27.03%)
    '     File Size: 1006 B


    ' Module Extensions
    ' 
    '     Function: (+2 Overloads) StemmerNormalize
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public Module Extensions

    <Extension>
    Public Iterator Function StemmerNormalize(docs As IEnumerable(Of String)) As IEnumerable(Of String)
        Dim s As New Stemmer()

        For Each str As String In docs
            Dim norm As New List(Of String)

            str = Strings.Trim(str).ToLower

            For Each token As String In str.Split
                For Each c As Char In token
                    Call s.add(c)
                Next

                Call norm.Add(s.ToString)
            Next

            Yield norm.JoinBy(" ")
        Next
    End Function

    Public Iterator Function StemmerNormalize(str As String) As IEnumerable(Of String)
        Dim s As New Stemmer()

        For Each token As String In Strings.Trim(str).ToLower.Split
            For Each c As Char In token
                Call s.add(c)
            Next

            Yield s.ToString
        Next
    End Function
End Module

