#Region "Microsoft.VisualBasic::a721d226be9e7488212fd43152ca6395, ..\visualbasic_App\Data_science\Mathematical\Math\BasicR\Solvers\SOR.vb"

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

'Namespace BasicR.Solvers

'    Public Class SOR : Implements BasicR.Solvers.ISolver

'#Region "Solver Controls"
'        ''' <summary>
'        ''' 松弛因子
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <XmlAttribute> Public Property Omiga As Double = 1.2
'        ''' <summary>
'        ''' 最大允许迭代次数
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <XmlAttribute> Public Property Iteration As Integer = 50
'        ''' <summary>
'        ''' 误差容限
'        ''' </summary>
'        ''' <value></value>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        <XmlAttribute> Public Property e As Double = 0.00000001
'#End Region

'        Public Function Solve(A As MATRIX, b As VECTOR) As VECTOR Implements ISolver.Solve
'            Dim N As Integer = A.Height
'            Dim x1 As VECTOR = New VECTOR(N), x As VECTOR = New VECTOR(N)

'            For k As Integer = 0 To Me.Iteration
'                For i As Integer = 0 To N - 1
'                    Dim sum As Double
'                    For j As Integer = 0 To N - 1
'                        If j < i Then
'                            sum += A(i, j) * x(j)
'                        ElseIf j > i Then
'                            sum += A(i, j) * x1(j)
'                        End If
'                    Next

'                    x(i) = (b(i) - sum) * Omiga / A(i, i) + (1.0 - Omiga) * x1(i)
'                Next
'#If DEBUG Then
'                Console.WriteLine(x.ToString)
'#End If
'                Dim dx As VECTOR = x - x1, err As Double = Math.Sqrt(dx.Mod)

'                If err < Me.e Then
'                    Exit For
'                End If

'                Call x.CopyTo(x1)
'            Next

'            Return x
'        End Function

'        Public Overrides Function ToString() As String
'            Return "BasicR -> Solver(SOR)"
'        End Function

'#Region "IDisposable Support"
'        Private disposedValue As Boolean ' 检测冗余的调用

'        ' IDisposable
'        Protected Overridable Sub Dispose(disposing As Boolean)
'            If Not Me.disposedValue Then
'                If disposing Then
'                    ' TODO:  释放托管状态(托管对象)。
'                End If

'                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
'                ' TODO:  将大型字段设置为 null。
'            End If
'            Me.disposedValue = True
'        End Sub

'        ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
'        'Protected Overrides Sub Finalize()
'        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
'        '    Dispose(False)
'        '    MyBase.Finalize()
'        'End Sub

'        ' Visual Basic 添加此代码是为了正确实现可处置模式。
'        Public Sub Dispose() Implements IDisposable.Dispose
'            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
'            Dispose(True)
'            GC.SuppressFinalize(Me)
'        End Sub
'#End Region
'    End Class
'End Namespace
