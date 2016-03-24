Imports System.Text

Namespace Language

    Public Class [Class](Of T) : Inherits ClassObject

        Public ReadOnly Property Type As Type

        Sub New()
            _Type = GetType(T)
        End Sub

        Public Overrides Function ToString() As String
            Return Type.FullName
        End Function

        Public Shared Operator <=(cls As [Class](Of T), path As String) As List(Of T)
            Dim source As IEnumerable = CollectionIO.DefaultLoadHandle(cls.Type, path, Encoding.Default)
            Return (From x In source Select DirectCast(x, T)).ToList
        End Operator

        Public Shared Operator >=(cls As [Class](Of T), path As String) As List(Of T)
            Throw New NotSupportedException
        End Operator

        Public Shared Operator <<(cls As [Class](Of T), path As Integer) As List(Of T)
            Return cls <= FileHandles.__getHandle(path)
        End Operator
    End Class
End Namespace