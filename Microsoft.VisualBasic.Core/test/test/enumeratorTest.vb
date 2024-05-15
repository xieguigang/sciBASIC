#Region "Microsoft.VisualBasic::1e81a12e8ea21d2a72fbb1916918043c, Microsoft.VisualBasic.Core\test\test\enumeratorTest.vb"

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

    '   Total Lines: 28
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 691 B


    ' Class enumeratorTest
    ' 
    '     Function: GenericEnumerator
    ' 
    ' Module enumeratorTestProgram
    ' 
    '     Sub: Mai2n
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class enumeratorTest : Implements Enumeration(Of String)

    Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
        Yield "1"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2"
        Yield "2000"
        Yield "29"
    End Function

End Class

Module enumeratorTestProgram

    Sub Mai2n()
        Dim strings As String() = New enumeratorTest().AsEnumerable.ToArray

        Call Console.WriteLine(strings.GetJson)

        Pause()
    End Sub
End Module
