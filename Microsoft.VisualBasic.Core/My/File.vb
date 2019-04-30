Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Text

Namespace My

    ''' <summary>
    ''' A internal file handler
    ''' </summary>
    Module File

        ReadOnly opendHandles As Dictionary(Of Integer, FileHandle)

        Sub New()
            opendHandles = New Dictionary(Of Integer, FileHandle)
        End Sub

        Friend Function GetHandle(path As Integer) As FileHandle
            If Not opendHandles.ContainsKey(path) Then
                Throw New ObjectNotFoundException($"Path {path} pointer to a null file handle!")
            Else
                Return opendHandles(path)
            End If
        End Function

        ''' <summary>
        ''' 不存在的文件句柄会在这个函数之中被忽略掉
        ''' </summary>
        ''' <param name="file%"></param>
        Public Sub Close(file%)
            SyncLock opendHandles
                With opendHandles
                    If .ContainsKey(file) Then
                        Call .Remove(file)
                    Else
                        ' Do Nothing.
                    End If
                End With
            End SyncLock
        End Sub

        Dim handle As Value(Of Integer) = New Value(Of Integer)(Integer.MinValue)

        ''' <summary>
        ''' Open a file system handle
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension> Public Function OpenHandle(file$, Optional encoding As Encodings = Encodings.UTF8) As Integer
            If String.IsNullOrEmpty(file) Then
                Throw New NullReferenceException("File handle null pointer!")
            End If

            SyncLock opendHandles
                SyncLock handle
                    My.File.handle.Value += 1

                    Dim handle As New FileHandle With {
                        .encoding = encoding.CodePage,
                        .FileName = file,
                        .handle = My.File.handle.Value
                    }

                    Call opendHandles.Add(My.File.handle.Value, handle)
                    Call FileIO.FileSystem.CreateDirectory(file.ParentPath)

                    Return My.File.handle.Value
                End SyncLock
            End SyncLock
        End Function

        Public Function OpenTemp() As Integer
            Return OpenHandle(App.GetAppSysTempFile(App.Process.Id))
        End Function

        ''' <summary>
        ''' Is this file opened
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        <Extension> Public Function FileOpened(filename As String) As Boolean
            If Not filename.FileExists Then
                Return False
            Else
                Try
                    Using file As New IO.FileStream(filename, IO.FileMode.OpenOrCreate)
                    End Using

                    Return False
                Catch ex As Exception
                    Return True
                Finally
                End Try
            End If
        End Function

        ''' <summary>
        ''' 等待文件句柄的关闭
        ''' </summary>
        ''' <param name="file$"></param>
        ''' <param name="timeout">等待的时间长度，默认为100s，单位为毫秒</param>
        ''' <returns></returns>
        Public Function Wait(file$, Optional timeout& = 1000 * 100) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew

            Do While file.FileOpened
                Call Thread.Sleep(1)

                If sw.ElapsedMilliseconds >= timeout Then
                    Return False
                End If
            Loop

            Return True
        End Function
    End Module
End Namespace

