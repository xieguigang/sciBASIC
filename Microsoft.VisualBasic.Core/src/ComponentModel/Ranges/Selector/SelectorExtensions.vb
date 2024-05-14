#Region "Microsoft.VisualBasic::b69e81435087deddd22021c4849209c7, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Selector\SelectorExtensions.vb"

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

    '   Total Lines: 20
    '    Code Lines: 16
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 737 B


    '     Module SelectorExtensions
    ' 
    '         Function: OrderSelector, Values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Ranges

    <HideModuleName>
    Public Module SelectorExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function OrderSelector(Of T As IComparable)(src As IEnumerable(Of T), Optional asc As Boolean = True) As OrderSelector(Of T)
            Return New OrderSelector(Of T)(src, asc)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Values(Of T)(numerics As IEnumerable(Of NumericTagged(Of T))) As IEnumerable(Of T)
            Return numerics.Select(Function(x) x.value)
        End Function
    End Module
End Namespace
