#Region "Microsoft.VisualBasic::2d61531d1d910aedc8f0560c13fce1a1, ..\R.Bioconductor\RDotNET.Extensions.VisualBasic\RSyntax\Vectors\GenericVector(Of T).vb"

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

Namespace SyntaxAPI.Vectors

    Public Class GenericVector(Of T) : Implements IEnumerable(Of T)
        Implements System.IDisposable

        ''' <summary>
        ''' 向量维数
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property [Dim] As Integer
            Get
                Return Elements.Length
            End Get
        End Property

        Public Property Elements As T()

        Default Public Overloads Property ElementWhere(conditions As BooleanVector) As T()
            Get
                Dim LQuery As T() = LinqAPI.Exec(Of T) <=
                    From i As SeqValue(Of Boolean)
                    In conditions.SeqIterator
                    Where i.obj = True
                    Select _Elements(i.i)

                Return LQuery
            End Get
            Set(value As T())
                For i As Integer = 0 To conditions.Count - 1
                    If conditions._Elements(i) Then
                        _Elements(i) = value(i)
                    End If
                Next
            End Set
        End Property

        Public Sub Factor(value As Integer)
            ReDim Preserve _Elements(value - 1)
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
                Dim x As Integer = a.Elements(0), y As Integer = b.Elements(0)
                Dim ChunkBuffer As T() = New T(y - x - 1) {}
                Call Array.ConstrainedCopy(Elements, x, ChunkBuffer, 0, ChunkBuffer.Length)
                Return ChunkBuffer
            End Get
            Set(value As T())
                Dim x As Integer = a.Elements(0), y As Integer = b.Elements(0)
                Dim idx As Integer = 0
                For i As Integer = x To y
                    _Elements(i) = value(idx)
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

#Region "Implements Generic.IEnumerable(Of T)"

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each x As T In Elements
                Yield x
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region

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
