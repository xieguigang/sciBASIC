Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Parallel

Namespace HoughCircles

    Public Class DetectEdge : Inherits VectorTask

        ReadOnly binarImg As Short(,)
        ReadOnly bb As Boolean(,)
        ReadOnly width As Integer
        ReadOnly height As Integer

        Shared ReadOnly gx As Integer(,) = New Integer(,) {{-1, 0, 1}, {-2, 0, 2}, {-1, 0, 1}}
        Shared ReadOnly gy As Integer(,) = New Integer(,) {{1, 2, 1}, {0, 0, 0}, {-1, -2, -1}}

        Public Sub New(binarImg As Short(,),
                       Optional verbose As Boolean = False,
                       Optional workers As Integer? = Nothing)

            Call MyBase.New(binarImg.GetLength(0), verbose, workers)

            Me.binarImg = binarImg
            Me.width = bb.GetLength(1)
            Me.height = bb.GetLength(0)
            Me.bb = New Boolean(height - 1, width - 1) {}
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getEdges() As Boolean(,)
            Return bb
        End Function

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim limit = 128 * 128
            Dim newX = 0, newY = 0, c = 0

            For Y As Integer = start + 1 To ends - 1 - 1
                For X As Integer = 1 To width - 1 - 1

                    newX = 0
                    newY = 0
                    c = 0

                    For hw = -1 To 1
                        For ww = -1 To 1
                            c = binarImg(Y + hw, X + ww)
                            newX += gx(hw + 1, ww + 1) * c
                            newY += gy(hw + 1, ww + 1) * c
                        Next
                    Next
                    If newX * newX + newY * newY > limit Then
                        bb(Y, X) = True
                    Else
                        bb(Y, X) = False
                    End If
                Next
            Next
        End Sub
    End Class
End Namespace