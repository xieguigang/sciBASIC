Imports System

Namespace HDBSCAN.Utils
    Public Class BitSet
        Private _bits As Boolean() = New Boolean(-1) {}

        Public Function [Get](pos As Integer) As Boolean
            Return pos < _bits.Length AndAlso _bits(pos)
        End Function

        Public Sub [Set](pos As Integer)
            Ensure(pos)
            _bits(pos) = True
        End Sub

        Private Sub Ensure(pos As Integer)
            If pos >= _bits.Length Then
                Dim nd = New Boolean(pos + 64 - 1) {}
                Array.Copy(_bits, 0, nd, 0, _bits.Length)
                _bits = nd
            End If
        End Sub
    End Class
End Namespace
