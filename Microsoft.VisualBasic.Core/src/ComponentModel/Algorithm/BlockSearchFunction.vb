#Region "Microsoft.VisualBasic::b4a5e85316550eee5b5a8e44cd6652ee, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BlockSearchFunction.vb"

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

    '   Total Lines: 221
    '    Code Lines: 151 (68.33%)
    ' Comment Lines: 35 (15.84%)
    '    - Xml Docs: 82.86%
    ' 
    '   Blank Lines: 35 (15.84%)
    '     File Size: 7.49 KB


    '     Structure Block
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetComparision, ToString
    ' 
    '     Structure SequenceTag
    ' 
    '         Function: ToString
    ' 
    '     Class BlockSearchFunction
    ' 
    '         Properties: Keys, raw, size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BuildIndex, getOrderSeq, Search
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace ComponentModel.Algorithm

    Friend Structure Block(Of T)

        Dim min As Double
        Dim max As Double
        Dim block As SequenceTag(Of T)()

        Friend Sub New(tmp As SequenceTag(Of T)())
            block = tmp
            min = block.First.tag
            max = block.Last.tag
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{min} ~ {max}] {block.Length} elements"
        End Function

        Friend Shared Function GetComparision() As Comparison(Of Block(Of T))
            Return Function(source, target)
                       ' target is the input data to search
                       Dim x As Double = target.min

                       If x > source.min AndAlso x < source.max Then
                           Return 0
                       ElseIf source.min < x Then
                           Return -1
                       Else
                           Return 1
                       End If
                   End Function
        End Function

    End Structure

    Friend Structure SequenceTag(Of T)

        Dim i As Integer
        Dim tag As Double
        Dim data As T

        Public Overrides Function ToString() As String
            Return $"[{i}] {tag}"
        End Function

    End Structure

    Public Class BlockSearchFunction(Of T)

        Dim binary As BinarySearchFunction(Of Block(Of T), Block(Of T))
        Dim eval As Func(Of T, Double)
        Dim tolerance As Double

        ''' <summary>
        ''' the input element pool count
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer

        ''' <summary>
        ''' the raw input sequence data, element order keeps the same with the input sequence.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property raw As T()

        ''' <summary>
        ''' get all keys which are evaluated from the input object
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Keys As Double()
            Get
                Return binary.rawOrder _
                    .Select(Function(b) b.block) _
                    .IteratesALL _
                    .Select(Function(i) i.tag) _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="eval"></param>
        ''' <param name="tolerance"></param>
        ''' <param name="factor"></param>
        ''' <param name="fuzzy">
        ''' 20221101       
        ''' works for the continues numeric sequence
        ''' </param>
        Sub New(data As IEnumerable(Of T),
                eval As Func(Of T, Double),
                tolerance As Double,
                Optional factor As Double = 2,
                Optional fuzzy As Boolean = False)

            Dim input = getOrderSeq(data, eval).ToArray

            Me.raw = input.Select(Function(i) i.data).ToArray
            Me.tolerance = tolerance
            Me.eval = eval
            Me.size = input.Length

            If size = 0 Then
                Return
            Else
                Me.binary = BuildIndex(input, tolerance, factor, fuzzy)
            End If
        End Sub

        Private Shared Function BuildIndex(input As SequenceTag(Of T)(),
                                           tolerance As Double,
                                           factor As Double,
                                           fuzzy As Boolean) As BinarySearchFunction(Of Block(Of T), Block(Of T))

            Dim blocks As New List(Of Block(Of T))
            Dim block As Block(Of T)
            Dim tmp As New List(Of SequenceTag(Of T))
            Dim min As Double = input.First.tag
            Dim max As Double = input.Last.tag
            Dim delta As Double = tolerance * factor
            Dim compares = Algorithm.Block(Of T).GetComparision

            For Each x In input
                If x.tag - min <= delta Then
                    tmp.Add(x)
                ElseIf tmp > 0 Then
                    block = New Block(Of T)(tmp.PopAll)
                    min = x.tag
                    blocks.Add(block)
                    tmp.Add(x)
                End If
            Next

            If tmp > 0 Then
                block = New Block(Of T)(tmp.PopAll)
                blocks.Add(block)
            End If

            Return New BinarySearchFunction(Of Block(Of T), Block(Of T))(
                source:=blocks,
                key:=Function(any) any,
                compares:=compares,
                allowFuzzy:=fuzzy
            )
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function getOrderSeq(data As IEnumerable(Of T), eval As Func(Of T, Double)) As IEnumerable(Of SequenceTag(Of T))
            Return data _
                .Select(Function(a) (a, eval(a))) _
                .SeqIterator _
                .OrderBy(Function(a) a.value.Item2) _
                .Select(Function(a)
                            Return New SequenceTag(Of T) With {
                                .i = a.i,
                                .data = a.value.a,
                                .tag = a.value.Item2
                            }
                        End Function)
        End Function

        ''' <summary>
        ''' query data with a given tolerance value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' this function returns an empty collection if no hits result
        ''' </returns>
        Public Iterator Function Search(x As T, Optional tolerance As Double? = Nothing) As IEnumerable(Of T)
            Dim wrap As New Block(Of T) With {.min = eval(x)}
            Dim i As Integer = -1

            ' has no data to query
            If size = 0 Then
                Return
            Else
                i = binary.BinarySearch(target:=wrap)
            End If

            If i = -1 Then
                Return
            ElseIf tolerance Is Nothing Then
                tolerance = Me.tolerance
            End If

            Dim joint As New List(Of SequenceTag(Of T))

            If i = 0 Then
                ' 0+1
                joint.AddRange(binary(0).block)

                If binary.size > 1 Then
                    Call joint.AddRange(binary(1).block)
                End If
            ElseIf i = binary.size - 1 Then
                ' -2 | -1
                joint.AddRange(binary(-1).block)
                joint.AddRange(binary(-2).block)
            Else
                ' i-1 | i | i+1
                joint.AddRange(binary(i - 1).block)
                joint.AddRange(binary(i).block)
                joint.AddRange(binary(i + 1).block)
            End If

            Dim val As Double = eval(x)

            For Each a As SequenceTag(Of T) In joint
                If stdNum.Abs(a.tag - val) <= tolerance Then
                    Yield a.data
                End If
            Next
        End Function
    End Class
End Namespace
