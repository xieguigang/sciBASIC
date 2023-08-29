Imports System.IO

Namespace Serialization.BinaryDumping

    Public Class ObjectOutputStream : Implements IDisposable

        Dim disposedValue As Boolean
        Dim stream As Stream

        Sub New(s As Stream)
            stream = s
        End Sub

        Public Sub WriteObject(obj As Object)

        End Sub

        Public Sub Flush()
            Call stream.Flush()
        End Sub

        Public Sub Close()
            Call stream.Close()
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Try
                        Call stream.Flush()
                        Call stream.Close()
                    Catch ex As Exception

                    End Try
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