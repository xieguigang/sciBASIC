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

    End Structure

    Public Class BlockSearchFunction(Of T)

        Dim binary As BinarySearchFunction(Of Block(Of T), Block(Of T))
        Dim eval As Func(Of T, Double)
        Dim tolerance As Double

        Sub New(data As IEnumerable(Of T), eval As Func(Of T, Double), tolerance As Double, Optional blockSize As Integer = 100)
            Dim input = getOrderSeq(data, eval).ToArray
            Dim blocks As New List(Of Block(Of T))
            Dim block As Block(Of T)
            Dim tmp As New List(Of SequenceTag(Of T))
            Dim min As Double = input.First.tag
            Dim max As Double = input.Last.tag
            Dim delta As Double = (max - min) / blockSize * 1.25
            Dim compares = Algorithm.Block(Of T).GetComparision

            For Each x In input
                If x.tag - min < delta Then
                    tmp.Add(x)
                ElseIf tmp > 0 Then
                    block = New Block(Of T)(tmp.PopAll)
                    min = block.min
                    blocks.Add(block)
                End If
            Next

            If tmp > 0 Then
                block = New Block(Of T)(tmp.PopAll)
                blocks.Add(block)
            End If

            Me.tolerance = tolerance
            Me.eval = eval
            Me.binary = New BinarySearchFunction(Of Block(Of T), Block(Of T))(blocks, Function(any) any, compares)
        End Sub

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
        ''' <returns></returns>
        Public Iterator Function Search(x As T) As IEnumerable(Of T)
            Dim wrap As New Block(Of T) With {.min = eval(x)}
            Dim i As Integer = binary.BinarySearch(target:=wrap)

            If i = -1 Then
                Return
            End If

            Dim joint As New List(Of SequenceTag(Of T))

            If i = 0 Then
                ' 0+1
                joint.AddRange(binary(0).block)
                joint.AddRange(binary(1).block)
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