#Region "Microsoft.VisualBasic::5804958548da23091ae2f38c9c45b45d, sciBASIC#\Data_science\Mathematica\Math\Math\Algebra\Vector\Class\Linq.vb"

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

    '   Total Lines: 71
    '    Code Lines: 44
    ' Comment Lines: 19
    '   Blank Lines: 8
    '     File Size: 2.88 KB


    '     Class Vector
    ' 
    '         Function: argsort, column_stack, (+2 Overloads) Where
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace LinearAlgebra

    Partial Class Vector

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="condition">
        ''' 当conditon的某个位置的为true时，输出x的对应位置的元素，否则选择y对应位置的元素；
        ''' </param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Vector, y As Vector) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x(index)
                           Else
                               Yield y(index)
                           End If
                       Next
                   End Function().AsVector
        End Function

        Public Shared Function Where(condition As IEnumerable(Of Boolean), x As Double, y As Double) As Vector
            Return Iterator Function() As IEnumerable(Of Double)
                       For Each index As SeqValue(Of Boolean) In condition.SeqIterator
                           If index.value Then
                               Yield x
                           Else
                               Yield y
                           End If
                       Next
                   End Function().AsVector
        End Function

        ''' <summary>
        ''' Perform an indirect sort along the given axis using the algorithm specified
        ''' by the `kind` keyword. It returns an array Of indices Of the same shape As
        ''' `a` that index data along the given axis in sorted order.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns>Returns the indices that would sort an array.</returns>
        ''' <example>
        ''' x = np.array([3, 1, 2])
        ''' np.argsort(x)
        ''' array([1, 2, 0])
        ''' </example>
        Public Shared Function argsort(data As IEnumerable(Of Double)) As Integer()
            Dim sort = From x In data.SeqIterator Select x Order By x.value
            Dim index = sort.Select(Function(x) x.i).ToArray

            Return index
        End Function

        Public Shared Iterator Function column_stack(ParamArray vectors As Vector()) As IEnumerable(Of Double())
            Dim maxL = vectors.Max(Function(vec) vec.Length)

#Disable Warning
            For i As Integer = 0 To maxL - 1
                Yield vectors _
                    .Select(Function(vec) vec.ElementAtOrDefault(i)) _
                    .ToArray
            Next
#Enable Warning
        End Function
    End Class
End Namespace
