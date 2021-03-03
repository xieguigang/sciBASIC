Imports System

Namespace Xdr
    Public MustInherit Class Reader
        Public ReadOnly ByteReader As IByteReader

        Protected Sub New(reader As IByteReader)
            ByteReader = reader
        End Sub

        Public Function Read(Of T)() As T
            Try
                Return CacheRead(Of T)()
            Catch ex As SystemException
                Throw MapException.ReadOne(GetType(T), ex)
            End Try
        End Function

        Protected MustOverride Function CacheRead(Of T)() As T

        Public Function ReadFix(Of T)(len As UInteger) As T
            Try
                Return CacheReadFix(Of T)(len)
            Catch ex As SystemException
                Throw MapException.ReadFix(GetType(T), len, ex)
            End Try
        End Function

        Protected MustOverride Function CacheReadFix(Of T)(len As UInteger) As T

        Public Function ReadVar(Of T)(max As UInteger) As T
            Try
                Return CacheReadVar(Of T)(max)
            Catch ex As SystemException
                Throw MapException.ReadVar(GetType(T), max, ex)
            End Try
        End Function

        Protected MustOverride Function CacheReadVar(Of T)(max As UInteger) As T

        Public Function ReadOption(Of T As Class)() As T
            If Read(Of Boolean)() Then
                Return Read(Of T)()
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
