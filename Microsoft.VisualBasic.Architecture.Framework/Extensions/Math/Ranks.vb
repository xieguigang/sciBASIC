#Region "Microsoft.VisualBasic::1e3370690e8a7af55a0a8abf425d208c, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Math\Ranks.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Mathematical

    Public Module Ranks

        Public Class Ranking(Of T)

            Public Property Evaluate As Func(Of T, Double)
            ''' <summary>
            ''' The sort direction
            ''' </summary>
            ''' <returns></returns>
            Public Property Max As Boolean
            ''' <summary>
            ''' 默认不加权重
            ''' </summary>
            ''' <returns></returns>
            Public Property Weight As Double = 1

            Public Function Sort(source As IEnumerable(Of T)) As SeqValue(Of T, Double)()
                Dim Evaluate As Func(Of T, Double) = Me.Evaluate
                Dim LQuery = (From x As T In source Select x, v = Evaluate(x)).ToArray
                Dim result As SeqValue(Of T, Double)()
                Dim weights As Double() = _Weight.CopyVector(LQuery.Length)

                If Max Then   ' 由于后面需要进行加权计算，所以在这里是反过来求最大的
                    result = (From x In LQuery Select x Order By x.v Ascending) _
                        .Select(Function(x) x.x) _
                        .SeqIterator(weights) _
                        .ToArray
                Else
                    result = (From x In LQuery Select x Order By x.v Descending) _
                        .Select(Function(x) x.x) _
                        .SeqIterator(weights) _
                        .ToArray
                End If

                Return result
            End Function
        End Class

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="Evaluate"></param>
        ''' <returns>Ranks值最小的是认为最好的</returns>
        Public Function Best(Of T)(source As IEnumerable(Of T), Evaluate As IEnumerable(Of Ranking(Of T))) As T
            Dim array As T() = Sort(source, Evaluate).ToArray
            Return array.FirstOrDefault
        End Function

        Public Function Sort(Of T)(source As IEnumerable(Of T), Evaluate As IEnumerable(Of Ranking(Of T))) As IEnumerable(Of T)
            Dim LQuery = (From method As Ranking(Of T) In Evaluate.AsParallel Select method.Sort(source)).IteratesALL
            Dim Groups = (From x In LQuery Select x Group x By x.value Into Group).ToArray
            Dim Ranks = (From x In Groups.AsParallel
                         Select x.value,
                         rank = x.Group.Sum(Function(o) o.i * o.Follows)  ' 加权重计算
                         Order By rank Descending).ToArray
            Return Ranks.Select(Function(x) x.value)
        End Function

    End Module
End Namespace
