#Region "Microsoft.VisualBasic::0c118f269e92d3295ecf727bc3e30ca3, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Pointer.vb"

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

    '   Total Lines: 33
    '    Code Lines: 17 (51.52%)
    ' Comment Lines: 10 (30.30%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (18.18%)
    '     File Size: 1.11 KB


    '     Class Pointer
    ' 
    '         Operators: (+2 Overloads) -, (+2 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' 进行集合之中的元素的取出操作的帮助类
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Pointer(Of T) : Inherits Pointer

        ''' <summary>
        ''' Returns current line in the array and then pointer moves to next.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(array As T(), i As Pointer(Of T)) As T
            Return array(+i)
        End Operator

        Public Overloads Shared Operator -(array As T(), i As Pointer(Of T)) As T
            Return array(-i)
        End Operator

        Public Overloads Shared Operator +(list As List(Of T), i As Pointer(Of T)) As T
            Return list(+i)
        End Operator

        Public Overloads Shared Operator -(list As List(Of T), i As Pointer(Of T)) As T
            Return list(-i)
        End Operator
    End Class
End Namespace
