#Region "Microsoft.VisualBasic::3fef618e0b4e8a5afc7d73c7a372a656, Microsoft.VisualBasic.Core\src\Language\Linq\Vectorization\Extensions.vb"

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

    '   Total Lines: 42
    '    Code Lines: 25 (59.52%)
    ' Comment Lines: 10 (23.81%)
    '    - Xml Docs: 70.00%
    ' 
    '   Blank Lines: 7 (16.67%)
    '     File Size: 1.52 KB


    '     Module Extensions
    ' 
    '         Function: Add, (+2 Overloads) ToVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Vectorization

    <HideModuleName>
    Public Module Extensions

        ' 2019-05-30 因为在这里的向量对象创建函数的名称
        ' 原来是AsVector，会和math模块中的AsVector产生冲突
        ' 所以这里都修改为ToVector

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(booleans As IEnumerable(Of Boolean)) As BooleanVector
            Return New BooleanVector(booleans)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToVector(Of T)(list As IEnumerable(Of T)) As Vector(Of T)
            Return New Vector(Of T)(list)
        End Function

        ''' <summary>
        ''' Dynamics add a element into the target array.(注意：不推荐使用这个函数来频繁的向数组中添加元素，这个函数会频繁的分配内存，效率非常低)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Add(Of T)(x As Vector(Of T), obj As T) As Vector(Of T)
            If x.buffer Is Nothing Then
                x.buffer = {obj}
            Else
                x.Array.Add(obj)
            End If

            Return x
        End Function
    End Module
End Namespace
