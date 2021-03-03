Imports System

Namespace Xdr
    Public MustInherit Class Writer
        Public ReadOnly ByteWriter As IByteWriter

        Protected Sub New(writer As IByteWriter)
            ByteWriter = writer
        End Sub

        Public Sub Write(Of T)(item As T)
            Try
                CacheWrite(item)
            Catch ex As SystemException
                Throw MapException.WriteOne(GetType(T), ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWrite(Of T)(item As T)

        Public Sub WriteFix(Of T)(len As UInteger, item As T)
            Try
                CacheWriteFix(len, item)
            Catch ex As SystemException
                Throw MapException.WriteFix(GetType(T), len, ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWriteFix(Of T)(len As UInteger, item As T)

        Public Sub WriteVar(Of T)(max As UInteger, item As T)
            Try
                CacheWriteVar(max, item)
            Catch ex As SystemException
                Throw MapException.WriteVar(GetType(T), max, ex)
            End Try
        End Sub

        Protected MustOverride Sub CacheWriteVar(Of T)(max As UInteger, item As T)

        Public Sub WriteOption(Of T As Class)(item As T)
            If item Is Nothing Then
                Write(False)
            Else
                Write(True)
                Write(item)
            End If
        End Sub
    End Class
End Namespace
