#Region "Microsoft.VisualBasic::81b3f215d912b8c9efdd501fd062166b, Data_science\Mathematica\Math\Math\Scripting\Factors\Extensions.vb"

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

    '   Total Lines: 53
    '    Code Lines: 39 (73.58%)
    ' Comment Lines: 7 (13.21%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (13.21%)
    '     File Size: 1.97 KB


    '     Module FactorExtensions
    ' 
    '         Properties: names
    ' 
    '         Function: factors
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

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

        Public Property names(vector As IFactorVector) As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vector.index.Keys.ToArray
            End Get
            Set(value As String())
                vector.index = value _
                    .SeqIterator _
                    .ToDictionary(Function(key) key.value,
                                  Function(index)
                                      Return index.i
                                  End Function)
            End Set
        End Property
    End Module
End Namespace
