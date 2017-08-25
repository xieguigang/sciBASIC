#Region "Microsoft.VisualBasic::58b026eaf44b779306431bff3512dfc3, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Factor.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Class Factor(Of T As IComparable(Of T)) : Inherits int
    Implements Value(Of T).IValueOf

    Public Property FactorValue As T Implements Value(Of T).IValueOf.Value

    Sub New()
    End Sub

    Sub New(value As T, factor%)
        Me.Value = factor
        Me.FactorValue = value
    End Sub

    Public Overrides Function ToString() As String
        Return FactorValue.ToString
    End Function

    Public Overloads Shared Narrowing Operator CType(factor As Factor(Of T)) As T
        Return factor.FactorValue
    End Operator
End Class

Public Class Factors(Of T As IComparable(Of T)) : Inherits Index(Of T)

    Sub New(ParamArray list As T())
        Call MyBase.New(list)
    End Sub

    Public Iterator Function GetFactors() As IEnumerable(Of Factor(Of T))
        For Each i In MyBase.Map
            Yield New Factor(Of T) With {
                .FactorValue = i.Key,
                .Value = i.Value
            }
        Next
    End Function
End Class

Public Module FactorExtensions

    ''' <summary>
    ''' 这个函数和<see cref="SeqIterator"/>类似，但是这个函数之中添加了去重和排序
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    <Extension>
    Public Function factors(Of T As IComparable(Of T))(source As IEnumerable(Of T)) As Factor(Of T)()
        Dim array = source.ToArray
        Dim unique As Index(Of T) = array _
            .Distinct _
            .OrderBy(Function(x) x) _
            .Indexing
        Dim out = array _
            .Select(Function(x)
                        Return New Factor(Of T) With {
                            .value = unique.IndexOf(x),
                            .FactorValue = x
                        }
                    End Function) _
            .ToArray
        Return out
    End Function
End Module
