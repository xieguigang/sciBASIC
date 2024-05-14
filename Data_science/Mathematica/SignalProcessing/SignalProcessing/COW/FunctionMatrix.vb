#Region "Microsoft.VisualBasic::ad1e0d82c3789343282d4d159b115a90, Data_science\Mathematica\SignalProcessing\SignalProcessing\COW\FunctionMatrix.vb"

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

    '   Total Lines: 75
    '    Code Lines: 47
    ' Comment Lines: 13
    '   Blank Lines: 15
    '     File Size: 2.84 KB


    '     Class FunctionMatrix
    ' 
    '         Properties: dims
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, Initialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace COW

    ''' <summary>
    ''' This class is used in dynamic programming algorithm of CowAlignment.cs
    ''' </summary>
    Friend Class FunctionMatrix : Implements IDisposable

        Dim funcBeanMatrix As FunctionElement()()
        Dim disposedValue As Boolean

        Public ReadOnly Property dims As (row As Integer, col As Integer)

        Default Public Property Item(rowPosition As Integer, columnPosition As Integer) As FunctionElement
            Get
                Return funcBeanMatrix(rowPosition)(columnPosition)
            End Get
            Set(value As FunctionElement)
                funcBeanMatrix(rowPosition)(columnPosition) = value
            End Set
        End Property

        Public Sub New(rowSize As Integer, columnSize As Integer)
            dims = (rowSize, columnSize)
            funcBeanMatrix = RectangularArray.Matrix(Of FunctionElement)(rowSize, columnSize)
        End Sub

        Public Sub Initialize(segmentNumber As Integer, enabledLength As Integer)
            Dim mat = funcBeanMatrix

            For i = 0 To segmentNumber
                For j = 0 To enabledLength
                    mat(i)(j) = New FunctionElement(Double.MinValue, 0)
                Next
            Next

            Me(segmentNumber, enabledLength).Score = 0
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{dims.row} x {dims.col}]"
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    For i As Integer = 0 To funcBeanMatrix.Length - 1
                        Erase funcBeanMatrix(i)
                    Next

                    Erase funcBeanMatrix
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
