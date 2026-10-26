Imports System.IO
Imports System.IO.Compression

Namespace Numpy

    ''' <summary>
    ''' numpy 的 <c>.npz</c> 归档：一个 zip 容器，内部每个条目都是一个完整的 <c>.npy</c> 文件。
    ''' </summary>
    ''' <remarks>
    ''' 读取时条目名会去掉 <c>.npy</c> 后缀作为数组名，并保持 zip 中的条目顺序；
    ''' 写入时按 <c>名称.npy</c> 生成条目，deflate 压缩。
    ''' 
    ''' 用法示例：
    ''' 
    ''' <code>
    ''' Dim npz = NpzFile.Read("demo.npz")
    ''' Dim matrix = npz("expression")
    ''' 
    ''' Dim out As New NpzFile()
    ''' Call out.Add("expression", matrix)
    ''' Call out.Save("copy.npz")
    ''' </code>
    ''' </remarks>
    Public Class NpzFile

        Private ReadOnly _names As New List(Of String)()
        Private ReadOnly _arrays As New Dictionary(Of String, NpyArray)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' 数组名称（按 zip 条目顺序）。
        ''' </summary>
        Public ReadOnly Property Names As String()
            Get
                Return _names.ToArray()
            End Get
        End Property

        ''' <summary>
        ''' 归档中的数组个数。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _names.Count
            End Get
        End Property

        ''' <summary>
        ''' 按名称获取数组（名称大小写不敏感，<c>.npy</c> 后缀可省略）。
        ''' </summary>
        ''' <param name="name">数组名称</param>
        Public Default ReadOnly Property Item(name As String) As NpyArray
            Get
                Dim key As String = NormalizeName(name)

                If _arrays.ContainsKey(key) Then
                    Return _arrays(key)
                End If

                Throw New KeyNotFoundException(
                    $"numpy archive does not contain an array named '{name}', available names: {String.Join(", ", _names)}")
            End Get
        End Property

        ''' <summary>
        ''' 判断归档中是否存在指定名称的数组。
        ''' </summary>
        ''' <param name="name">数组名称</param>
        Public Function Contains(name As String) As Boolean
            Return _arrays.ContainsKey(NormalizeName(name))
        End Function

        ''' <summary>
        ''' 添加（或替换）一个数组。
        ''' </summary>
        ''' <param name="name">数组名称</param>
        ''' <param name="array">数组数据</param>
        Public Sub Add(name As String, array As NpyArray)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("numpy array name can not be empty!", NameOf(name))
            End If
            If array Is Nothing Then
                Throw New ArgumentNullException(NameOf(array))
            End If

            Dim key As String = NormalizeName(name)
            Dim old As String = _names.FirstOrDefault(Function(n) String.Equals(n, key, StringComparison.OrdinalIgnoreCase))

            If old IsNot Nothing Then
                _names.Remove(old)
            End If

            Call _names.Add(key)

            array.Name = key
            _arrays(key) = array
        End Sub

        ''' <summary>
        ''' 读取 npz 文件。
        ''' </summary>
        ''' <param name="path">npz 文件路径</param>
        Public Shared Function Read(path As String) As NpzFile
            Using stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return Read(stream)
            End Using
        End Function

        ''' <summary>
        ''' 从流中读取 npz 归档。
        ''' </summary>
        ''' <param name="stream">zip 数据流</param>
        Public Shared Function Read(stream As Stream) As NpzFile
            Dim result As New NpzFile()

            Using zip As New ZipArchive(stream, ZipArchiveMode.Read, leaveOpen:=True)
                For Each entry As ZipArchiveEntry In zip.Entries
                    If Not entry.FullName.EndsWith(".npy", StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Dim key As String = entry.FullName.Substring(0, entry.FullName.Length - 4)
                    Dim array As NpyArray

                    Using data As Stream = entry.Open()
                        array = NpyFile.Read(data)
                    End Using

                    Call result.Add(key, array)
                Next
            End Using

            Return result
        End Function

        ''' <summary>
        ''' 把归档写出到文件。
        ''' </summary>
        ''' <param name="path">输出文件路径；同名文件会被覆盖</param>
        Public Sub Save(path As String)
            Using stream As Stream = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                Call Save(stream)
            End Using
        End Sub

        ''' <summary>
        ''' 把归档写出到流。
        ''' </summary>
        ''' <param name="stream">输出流</param>
        Public Sub Save(stream As Stream)
            If _names.Count = 0 Then
                Throw New InvalidOperationException("numpy archive is empty: at least one array is required!")
            End If

            Using zip As New ZipArchive(stream, ZipArchiveMode.Create, leaveOpen:=True)
                For Each key As String In _names
                    Dim entry As ZipArchiveEntry = zip.CreateEntry(key & ".npy", CompressionLevel.Optimal)

                    Using data As Stream = entry.Open()
                        Call NpyFile.WriteTo(data, _arrays(key))
                    End Using
                Next
            End Using
        End Sub

        ''' <summary>
        ''' 一次性把多个数组写成 npz 文件（数组名取自 <see cref="NpyArray.Name"/>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="arrays">数组集合</param>
        Public Shared Sub Write(path As String, ParamArray arrays As NpyArray())
            If arrays Is Nothing OrElse arrays.Length = 0 Then
                Throw New ArgumentException("at least one numpy array is required!", NameOf(arrays))
            End If

            Dim npz As New NpzFile()

            For i As Integer = 0 To arrays.Length - 1
                Dim [name] As String = arrays(i).Name

                If String.IsNullOrEmpty([name]) Then
                    [name] = $"arr_{i}"
                End If

                Call npz.Add([name], arrays(i))
            Next

            Call npz.Save(path)
        End Sub

        Private Shared Function NormalizeName(name As String) As String
            If name Is Nothing Then
                Throw New ArgumentNullException(NameOf(name))
            End If

            Dim key As String = name.Trim()

            If key.EndsWith(".npy", StringComparison.OrdinalIgnoreCase) Then
                key = key.Substring(0, key.Length - 4)
            End If

            Return key
        End Function

        ''' <summary>
        ''' 返回 <c>npz[name, name, ...]</c> 形式的摘要文本。
        ''' </summary>
        Public Overrides Function ToString() As String
            Return $"npz[{String.Join(", ", _names)}]"
        End Function

    End Class

End Namespace
