#Region "Microsoft.VisualBasic::2f0e8f2686e7e1c0e93ba3b99d544841, sciBASIC#\Data_science\MachineLearning\CellularAutomaton\Debugger\Debugger.vb"

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

    '   Total Lines: 80
    '    Code Lines: 53
    ' Comment Lines: 14
    '   Blank Lines: 13
    '     File Size: 2.75 KB


    ' Class Debugger
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, TakeSnapshots
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq

Public Class Debugger(Of T As Individual) : Implements IDisposable

    Dim cache As List(Of Integer)()()
    Dim getInt As ToInteger(Of T)
    Dim file As String
    Dim simulator As Simulator(Of T)
    Dim size As Size

    Sub New(file As String, simulator As Simulator(Of T), getInt As ToInteger(Of T))
        Me.size = simulator.size
        Me.file = file
        Me.getInt = getInt
        Me.simulator = simulator
        Me.cache = MAT(Of List(Of Integer))(size.Height, size.Width)

        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                cache(i)(j) = New List(Of Integer)
            Next
        Next
    End Sub

    Public Sub TakeSnapshots()
        For j As Integer = 0 To size.Width - 1
            For i As Integer = 0 To size.Height - 1
                cache(i)(j).Add(getInt(simulator.grid(i)(j).data))
            Next
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return file
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call cache _
                    .Select(Function(l)
                                Return l.Select(Function(x) x.ToArray).ToArray
                            End Function) _
                    .DoCall(Sub(matrix)
                                WriteCDF.Flush(file, matrix.ToArray, GetType(T))
                            End Sub)

                Call cache.Free
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
