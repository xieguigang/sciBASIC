Imports System

Namespace Xdr
    Friend NotInheritable Class ErrorStubType(Of T)
        Public ReadOnly [Error] As Exception

        Public Sub New(ex As Exception)
            [Error] = ex
        End Sub

        Public Function ReadOne(reader As Reader) As T
            Throw [Error]
        End Function

        Public Function ReadMany(reader As Reader, len As UInteger) As T
            Throw [Error]
        End Function

        Public Sub WriteOne(writer As Writer, v As T)
            Throw [Error]
        End Sub

        Public Sub WriteMany(writer As Writer, len As UInteger, v As T)
            Throw [Error]
        End Sub
    End Class
End Namespace
