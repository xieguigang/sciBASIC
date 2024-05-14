#Region "Microsoft.VisualBasic::445673a59c27b0978bcb6564318abffe, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\JoinElement.vb"

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

    '   Total Lines: 29
    '    Code Lines: 25
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 1.10 KB


    '     Class JoinElement
    ' 
    '         Properties: [next], element, index, previous
    ' 
    '         Function: FindElement, GetByIndex
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection

    Public Class JoinElement(Of T)

        Public Property previous As T
        Public Property index As Integer
        Public Property element As T
        Public Property [next] As T

        Public Shared Function GetByIndex(array As T(), index As Integer) As JoinElement(Of T)
            Return New JoinElement(Of T) With {
                .index = index,
                .element = array(index),
                .[next] = If(index = array.Length - 1, Nothing, array(index + 1)),
                .previous = If(index = 0, Nothing, array(index - 1))
            }
        End Function

        Public Shared Iterator Function FindElement(array As T(), find As Func(Of T, Boolean)) As IEnumerable(Of JoinElement(Of T))
            If Not array.IsNullOrEmpty Then
                For i As Integer = 0 To array.Length - 1
                    If find(array(i)) Then
                        Yield GetByIndex(array, i)
                    End If
                Next
            End If
        End Function
    End Class
End Namespace
