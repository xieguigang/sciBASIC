#Region "Microsoft.VisualBasic::a73a354d0375f71e5a73eb5ba467fb4e, ..\visualbasic_App\Data_science\Mathematical\Math\BasicR\RSyntax\Vectors\GenericVector(Of T).vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Namespace SyntaxAPI.Vectors

    Public Class GenericVector(Of T) : Inherits List(Of T)
        Implements IEnumerable(Of T)
        Implements IDisposable

        ''' <summary>
        ''' 向量维数
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Dim%
            Get
                Return Count
            End Get
        End Property

        Sub New()
            MyBase.New
        End Sub

        Sub New(data As IEnumerable(Of T))
            MyBase.New(data)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the generic type vector class that
        ''' is empty and has the specified initial capacity.
        ''' </summary>
        ''' <param name="capacity">
        ''' The number of elements that the new list can initially store.
        ''' </param>
        Sub New(capacity%)
            MyBase.New(capacity)
        End Sub

        Public Overloads Property SelectWhere(conditions As BooleanVector) As T()
            Get
                Dim LQuery As T() = LinqAPI.Exec(Of T) <=
                    From i As SeqValue(Of Boolean)
                    In conditions.SeqIterator
                    Where i.obj = True
                    Select MyBase.Item(i.i)

                Return LQuery
            End Get
            Set(value As T())
                For i As Integer = 0 To conditions.Count - 1
                    If conditions(i) Then
                        Me(i) = value(i)
                    End If
                Next
            End Set
        End Property

        Public Sub Factor(value%)
            ' ReDim Preserve _Elements(value - 1)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a">只有一个元素的</param>
        ''' <param name="b">只有一个元素的</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Property [GET](a As Vector, b As Vector) As T()
            Get
                Dim x As Integer = a(0), y As Integer = b(0)
                Dim ChunkBuffer As T() = New T(y - x - 1) {}
                Call Array.ConstrainedCopy(Me.ToArray, x, ChunkBuffer, 0, ChunkBuffer.Length)
                Return ChunkBuffer
            End Get
            Set(value As T())
                Dim x As Integer = a(0), y As Integer = b(0)
                Dim idx As Integer = 0
                For i As Integer = x To y
                    Me(i) = value(idx)
                    idx += 1
                Next
            End Set
        End Property

        Public Overloads Property [GET](a As Integer, b As Vector) As T()
            Get
                Return [GET](New Vector({a}), b)
            End Get
            Set(value As T())
                [GET](New Vector({a}), b) = value
            End Set
        End Property

        Public Shared Operator <>(x As GenericVector(Of T), y As GenericVector(Of T)) As BooleanVector
            Dim LQuery = (From i In x.SeqIterator Select Not i.obj.Equals(y(i.i))).ToArray
            Return New BooleanVector(LQuery)
        End Operator

        Public Shared Operator =(x As GenericVector(Of T), y As GenericVector(Of T)) As BooleanVector
            Return Not (x <> y)
        End Operator

        Public Overloads Shared Narrowing Operator CType(v As GenericVector(Of T)) As T()
            Return v.ToArray
        End Operator

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
