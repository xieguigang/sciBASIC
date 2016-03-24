Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel

Namespace Language

    Public MustInherit Class File : Inherits ClassObject
        Implements ISaveHandle

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.GetEncodings)
        End Function

        Public MustOverride Function Save(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save

        Public Shared Operator >(file As File, path As String) As Boolean
            Return file.Save(path, Encodings.UTF8)
        End Operator

        Public Shared Operator <(file As File, path As String) As Boolean
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >>(file As File, path As Integer) As Boolean
            Return file.Save(__getHandle(path), Encodings.UTF8)
        End Operator
    End Class

    Public Module FileHandles

        Friend ReadOnly ___opendHandles As Dictionary(Of Integer, String)

        Friend Function __getHandle(path As Integer) As String
            If Not FileHandles.___opendHandles.ContainsKey(path) Then
                Throw New ObjectNotFoundException($"Path {path} pointer to a null file handle!")
            Else
                Return FileHandles.___opendHandles(path)
            End If
        End Function

        Public Sub Close(file As Integer)
            If ___opendHandles.ContainsKey(file) Then
                Call ___opendHandles.Remove(file)
            Else
                ' Do Nothing.
            End If
        End Sub

        Dim __handle As Value(Of Integer) = New Value(Of Integer)(Integer.MinValue)

        Public Function OpenHandle(file As String) As Integer
            SyncLock ___opendHandles
                SyncLock __handle
                    __handle.Value += 1
                    Call ___opendHandles.Add(__handle.Value, file)
                    Call FileIO.FileSystem.CreateDirectory(file.ParentPath)

                    Return __handle.Value
                End SyncLock
            End SyncLock
        End Function

    End Module
End Namespace