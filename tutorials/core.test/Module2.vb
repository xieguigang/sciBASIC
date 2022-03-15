#Region "Microsoft.VisualBasic::5e8d1f4cbf48d04d3aa303a224d63a8a, sciBASIC#\tutorials\core.test\Module2.vb"

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

    '   Total Lines: 26
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 516.00 B


    ' Module Module2
    ' 
    '     Function: populateNothing
    ' 
    '     Sub: Main
    '     Class List
    ' 
    '         Properties: data
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Module Module2

    Sub Main()

        Dim nnn = TryCast(Nothing, String()).SafeQuery

        Dim list = populateNothing()?.data

        For Each s In list.SafeQuery
        Next
        For Each s In populateNothing()?.data.SafeQuery
        Next

        Pause()
    End Sub

    Public Class List
        Public Property data As String()
    End Class

    Public Function populateNothing() As List
        Return Nothing
    End Function
End Module
