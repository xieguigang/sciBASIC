Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Calculus

Namespace MonteCarlo

    ''' <summary>
    ''' Sample partial reference model
    ''' </summary>
    Public MustInherit Class RefModel : Inherits Model

        Public Property Delta As Integer
        Public Property RefValues As ValueVector

        Dim RK%
        Dim d%

        ''' <summary>
        ''' RK4每一次迭代会调用这个函数计算4次
        ''' </summary>
        ''' <param name="dx"></param>
        ''' <param name="dy"></param>
        Protected NotOverridable Overrides Sub func(dx As Double, ByRef dy As Vector)
            RK += 1
            func(dx, dy, RefValues)

            If RK = 4 Then
                RK = 0
                d += 1
                If d > Delta Then
                    RefValues += 1  ' 最开始是0，假若这句代码被放在func调用的前面首先自增1的话，会在末尾出现越界的问题
                    d = 0
                End If
            End If
        End Sub

        Protected MustOverride Overloads Sub func(dx#, ByRef dy As Vector, Y As ValueVector)
    End Class
End Namespace