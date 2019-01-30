Namespace Layouts.Cola

    Public Class Blocks : Implements IEnumerable(Of Block)

        Dim list As New List(Of Block)
        Public vs As Variable()

        Public ReadOnly Property cost As Double
            Get
                Return Aggregate b In list.ReverseIterator Into Sum(b.cost)
            End Get
        End Property

        Sub New(vs As Variable())
            Dim n As int = vs.Length

            While (--n)
                Dim b As New Block(vs(n))
                list(CInt(n)) = b
                b.blockInd = n
            End While
        End Sub

        Public Sub insert(b As Block)

            b.blockInd = list.Count
            list.Add(b)

        End Sub

        Public Sub remove(b As Block)
            Dim last = list.Count - 1
            Dim swapBlock = list(last)

            list.RemoveLast

            If (Not b Is swapBlock) Then
                list(b.blockInd) = swapBlock
                swapBlock.blockInd = b.blockInd
            End If
        End Sub

        ''' <summary>
        ''' merge the blocks on either side of the specified constraint, by copying the smaller block into the larger
        ''' And deleting the smaller.
        ''' </summary>
        ''' <param name="c"></param>
        Public Sub merge(c As Constraint)
            Dim l = c.left.block, r = c.right.block
            Dim dist = c.right.offset - c.left.offset - c.gap

            If (l.vars.Count < r.vars.Count) Then
                r.mergeAcross(l, c, dist)
                remove(l)
            Else
                l.mergeAcross(r, c, -dist)
                remove(r)
            End If
        End Sub

        ''' <summary>
        ''' useful, for example, after variable desired positions change.
        ''' </summary>
        Public Sub updateBlockPositions()
            For Each b In list
                b.updateWeightedPosition()
            Next
        End Sub

        ''' <summary>
        ''' split each block across its constraint with the minimum lagrangian
        ''' </summary>
        ''' <param name="inactive"></param>
        Public Sub split(ByRef inactive As List(Of Constraint))
            updateBlockPositions()
            For Each b In list
                Dim v = b.findMinLM()
                If (Not v Is Nothing AndAlso v.lm < Solver.LAGRANGIAN_TOLERANCE) Then
                    b = v.left.block
                    For Each nb In Block.split(v)
                        insert(nb)
                    Next

                    remove(b)
                    inactive.Add(v)

                End If
            Next
        End Sub

        ''' <summary>
        ''' For Each
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Block) Implements IEnumerable(Of Block).GetEnumerator
            For Each b As Block In list
                Yield b
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class


End Namespace