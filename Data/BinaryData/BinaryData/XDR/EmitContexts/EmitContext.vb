Imports System
Imports System.Collections.Generic

Namespace Xdr.EmitContexts
    Public Module EmitContext
        Private _sync As Object = New Object()
        Private _readerCache As Dictionary(Of Type, EmitResult) = New Dictionary(Of Type, EmitResult)()
        Private _writerCache As Dictionary(Of Type, EmitResult) = New Dictionary(Of Type, EmitResult)()

        Public Function GetReader(targetType As Type) As [Delegate]
            SyncLock _sync
                Dim result As EmitResult = Nothing

                If Not _readerCache.TryGetValue(targetType, result) Then
                    result = New EmitResult()

                    Try
                        result.Method = EmitReader(targetType)
                    Catch ex As Exception
                        result.Error = New InvalidOperationException("can't emit reader", ex)
                    End Try

                    _readerCache.Add(targetType, result)
                End If

                If result.Error IsNot Nothing Then Throw result.Error
                Return result.Method
            End SyncLock
        End Function

        Public Function GetWriter(targetType As Type) As [Delegate]
            SyncLock _sync
                Dim result As EmitResult = Nothing

                If Not _writerCache.TryGetValue(targetType, result) Then
                    result = New EmitResult()

                    Try
                        result.Method = EmitWriter(targetType)
                    Catch ex As Exception
                        result.Error = New InvalidOperationException("can't emit writer", ex)
                    End Try

                    _writerCache.Add(targetType, result)
                End If

                If result.Error IsNot Nothing Then Throw result.Error
                Return result.Method
            End SyncLock
        End Function

        Public Function EmitReader(targetType As Type) As [Delegate]
            Dim ordModel = OrderModel.Create(targetType)
            Dim swModel = SwitchModel.Create(targetType)
            If swModel IsNot Nothing AndAlso ordModel IsNot Nothing Then Throw New InvalidOperationException("unknown way to convert")
            If swModel IsNot Nothing Then Return swModel.BuildReader(targetType)
            If ordModel IsNot Nothing Then Return ordModel.BuildReader(targetType)
            Return Nothing
        End Function

        Public Function EmitWriter(targetType As Type) As [Delegate]
            Dim ordModel = OrderModel.Create(targetType)
            Dim swModel = SwitchModel.Create(targetType)
            If swModel IsNot Nothing AndAlso ordModel IsNot Nothing Then Throw New InvalidOperationException("unknown way to convert")
            If swModel IsNot Nothing Then Return swModel.BuildWriter(targetType)
            If ordModel IsNot Nothing Then Return ordModel.BuildWriter(targetType)
            Return Nothing
        End Function
    End Module
End Namespace
