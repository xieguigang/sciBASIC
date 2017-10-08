#Region "Microsoft.VisualBasic::e5eb5d17fab7f4a9aa3e551310904068, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Scripting\Factors\Extensions.vb"

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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Scripting

    Public Module FactorExtensions

        ''' <summary>
        ''' 这个函数和<see cref="SeqIterator"/>类似，但是这个函数之中添加了去重和排序
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="step">取默认值1是为了保持与Integer类型的index的兼容</param>
        ''' <returns></returns>
        <Extension>
        Public Function factors(Of T As IComparable(Of T))(source As IEnumerable(Of T), Optional step! = 1) As Factor(Of T)()
            Dim array = source.ToArray
            Dim unique As IEnumerable(Of T) = array _
                .Distinct _
                .OrderBy(Function(x) x)
            Dim factorValues As New Dictionary(Of T, Factor(Of T))
            Dim y#

            For Each x As T In unique
                factorValues.Add(x, New Factor(Of T)(x, y))
                y += [step]
            Next

            Dim out As Factor(Of T)() = array _
                .Select(Function(x)
                            Return New Factor(Of T)(x, factorValues(x).Value)
                        End Function) _
                .ToArray

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsCharacter(values As Dictionary(Of String, Double)) As Dictionary(Of String, String)
            Return values.ToDictionary(
                Function(x) x.Key,
                Function(x) CStr(x.Value))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsNumeric(values As Dictionary(Of String, String)) As Dictionary(Of String, Double)
            Return values.ToDictionary(
                Function(x) x.Key,
                Function(x) x.Value.ParseNumeric)
        End Function

        Public Property names(vector As FactorVector) As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vector.index.Keys.ToArray
            End Get
            Set(value As String())
                vector.index = value _
                    .SeqIterator _
                    .ToDictionary(Function(key) key.value,
                                  Function(index) index.i)
            End Set
        End Property
    End Module
End Namespace
