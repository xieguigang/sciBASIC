#Region "Microsoft.VisualBasic::466091d693617e1e941716a2c1fa5628, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\Selector\IndexSelector.vb"

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

    '   Total Lines: 48
    '    Code Lines: 27
    ' Comment Lines: 14
    '   Blank Lines: 7
    '     File Size: 2.04 KB


    '     Class IndexSelector
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: FromSortSequence, SelectByRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Numeric = Microsoft.VisualBasic.Language.Numeric

Namespace ComponentModel.Ranges

    ''' <summary>
    ''' A numeric index helper
    ''' </summary>
    Public Class IndexSelector : Inherits OrderSelector(Of NumericTagged(Of Integer))

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(source As IEnumerable(Of Double), Optional asc As Boolean = True)
            MyBase.New(source.Select(Function(d, i) New NumericTagged(Of Integer)(d, i)), asc)
        End Sub

        Private Sub New(sorts As IEnumerable(Of NumericTagged(Of Integer)), asc As Boolean)
            Call MyBase.New({}, asc)
            source = sorts.ToArray
        End Sub

        ''' <summary>
        ''' Get index by a given numeric range
        ''' </summary>
        ''' <param name="min"></param>
        ''' <param name="max"></param>
        ''' <returns></returns>
        Public Overloads Function SelectByRange(min As Double, max As Double) As IEnumerable(Of Integer)
            Dim minValue As New NumericTagged(Of Integer)(min, 0)
            Dim maxvalue As New NumericTagged(Of Integer)(max, 0)

            Return source.SkipWhile(Function(o) Numeric.LessThan(o, minValue)) _
                         .TakeWhile(Function(o) Numeric.LessThanOrEquals(o, maxvalue)) _
                         .Select(Function(tag)
                                     Return tag.value
                                 End Function)
        End Function

        ''' <summary>
        ''' 所使用的序列参数必须是经过了排序操作的
        ''' </summary>
        ''' <param name="seq"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromSortSequence(seq As Double()) As IndexSelector
            Return New IndexSelector(seq.Select(Function(d, i) New NumericTagged(Of Integer)(d, i)), True)
        End Function
    End Class
End Namespace
