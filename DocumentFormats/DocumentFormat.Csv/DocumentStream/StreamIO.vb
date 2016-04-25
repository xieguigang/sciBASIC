Imports System.IO
Imports System.Text

Namespace DocumentStream

    Public Module StreamIO

        ''' <summary>
        ''' Save this csv document into a specific file location <paramref name="path"/>.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="lazySaved">Optional, this is for the consideration of performance and memory consumption.
        ''' When a data file is very large, then you may encounter a out of memory exception on a 32 bit platform,
        ''' then you should set this parameter to True to avoid this problem. Defualt is False for have a better
        ''' performance.
        ''' (当估计到文件的数据量很大的时候，请使用本参数，以避免内存溢出致使应用程序崩溃，默认为False，不开启缓存)
        ''' </param>
        ''' <remarks>当目标保存路径不存在的时候，会自动创建文件夹</remarks>
        Public Function SaveDataFrame(df As File,
                                      Optional path As String = "",
                                      Optional lazySaved As Boolean = False,
                                      Optional encoding As Encoding = Nothing) As Boolean

            If String.IsNullOrEmpty(path) Then
                Throw New NullReferenceException("path reference to a null location!")
            End If
            If encoding Is Nothing Then
                encoding = Encoding.UTF8
            End If

            If lazySaved Then
                Return __lazySaved(path, df, encoding)
            End If

            Dim stopwatch = System.Diagnostics.Stopwatch.StartNew
            Dim text As String = df.Generate

            Call Console.WriteLine("Generate csv file document using time {0} ms.", stopwatch.ElapsedMilliseconds)

            Return text.SaveTo(path, encoding)
        End Function

        ''' <summary>
        ''' 在保存大文件时为了防止在保存的过程中出现内存溢出所使用的一种保存方法
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="df"></param>
        ''' <param name="encoding"></param>
        ''' <remarks></remarks>
        Private Function __lazySaved(path As String, df As File, encoding As Encoding) As Boolean
            Call Console.WriteLine("Open csv file handle, and writing chunk buffer into file...")
            Call Console.WriteLine("Object counts is ""{0}""", df._innerTable.Count)

            Call "".SaveTo(path)

            Try
                Dim rowBuffer As RowObject() = df.__createTableVector
                Return __lazyInner(path, rowBuffer, encoding)
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
                Return False
            End Try
        End Function

        Private Function __lazyInner(filepath As String, rowBuffer As RowObject(), encoding As Encoding) As Boolean
            Dim stopWatch = System.Diagnostics.Stopwatch.StartNew
            Dim chunks As RowObject()() = rowBuffer.Split(10240)
            Dim handle As IO.FileStream =
                IO.File.Open(filepath,
                             IO.FileMode.OpenOrCreate,
                             IO.FileAccess.ReadWrite)

            Using writer As New StreamWriter(handle, encoding)
                For Each block As RowObject() In chunks
                    Dim sBlock As String = (From row As RowObject
                                            In block.AsParallel
                                            Select row.AsLine).JoinBy(vbCrLf)
                    Call writer.WriteLine(sBlock)
                    Call Console.Write(".")
                Next
            End Using

            Call $"Write csv data file cost time {stopWatch.ElapsedMilliseconds} ms.".__DEBUG_ECHO

            Return True
        End Function
    End Module
End Namespace