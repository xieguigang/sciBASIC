#Region "Microsoft.VisualBasic::ee92ebd031e9d4bcb23741338f3cd0a0, Microsoft.VisualBasic.Core\src\Extensions\IO\Extensions\IO.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 360
    '    Code Lines: 192 (53.33%)
    ' Comment Lines: 132 (36.67%)
    '    - Xml Docs: 78.79%
    ' 
    '   Blank Lines: 36 (10.00%)
    '     File Size: 14.25 KB


    ' Module IOExtensions
    ' 
    '     Function: FixPath, FlushAllLines, (+3 Overloads) FlushStream, Open, OpenReader
    '               OpenReadonly, OpenTextWriter, ReadBinary, ReadVector
    ' 
    '     Sub: ClearFileBytes, FlushTo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Unit
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports ASCII = Microsoft.VisualBasic.Text.ASCII

''' <summary>
''' The extension API for system file io.
''' </summary>
''' <remarks>
''' (IO函数拓展)
''' </remarks>
<Package("IO")>
Public Module IOExtensions

    Public ReadOnly UTF8 As [Default](Of Encoding) = Encoding.UTF8

    ''' <summary>
    ''' Open text writer interface from a given <see cref="Stream"/> <paramref name="s"/>. 
    ''' </summary>
    ''' <param name="s"></param>
    ''' <param name="encoding">By default is using <see cref="UTF8"/> text encoding.</param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function OpenTextWriter(s As Stream, Optional encoding As Encoding = Nothing) As StreamWriter
        Return New StreamWriter(s, encoding Or UTF8) With {
            .NewLine = ASCII.LF
        }
    End Function

    ''' <summary>
    ''' copy the data from the input <paramref name="stream"/> to 
    ''' the target file which is specified by the parameter
    ''' <paramref name="path"/>
    ''' </summary>
    ''' <param name="stream">
    ''' 必须要能够支持<see cref="Stream.Length"/>，对于有些网络服务器的HttpResponseStream可能不支持
    ''' <see cref="Stream.Length"/>的话，这个函数将会报错
    ''' </param>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function FlushStream(stream As Stream, path$) As Boolean
        Using writer As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Call stream.CopyTo(writer)
            Call writer.Flush()
            Call writer.Close()
        End Using

        Return True
    End Function

    ''' <summary>
    ''' 将指定的字符串的数据值写入到目标可写的输出流之中
    ''' </summary>
    ''' <param name="data$">所需要写入的字符串数据</param>
    ''' <param name="out">输出流</param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub FlushTo(data$, out As StreamWriter, Optional closeFile As Boolean = False)
        Call out.WriteLine(data)

        If closeFile Then
            Call out.Flush()
            Call out.Dispose()
        End If
    End Sub

    ''' <summary>
    ''' 为了方便在linux上面使用，这里会处理一下file://这种情况，请注意参数是ByRef引用的
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Public Function FixPath(ByRef path$) As String
        If InStr(path, "file://", CompareMethod.Text) = 1 Then
            If App.IsMicrosoftPlatform AndAlso InStr(path, "file:///", CompareMethod.Text) = 1 Then
                path = Mid(path, 9)
            Else
                path = Mid(path, 8)
            End If
        Else
            path = FileIO.FileSystem.GetFileInfo(path).FullName
        End If

        Return path$
    End Function

    ''' <summary>
    ''' Read target text file as a numeric vector, each line in the target text file should be a number, 
    ''' so that if the target text file have n lines, then the returned vector have n elements.
    ''' (这个文本文件之中的每一行都是一个数字，所以假设这个文本文件有n行，那么所返回的向量的长度就是n)
    ''' </summary>
    ''' <param name="path">The file path of the target text file.</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ReadVector(path As String) As Double()
        Return IO.File.ReadAllLines(path) _
            .Select(Function(x) CDbl(x)) _
            .ToArray
    End Function

    ''' <summary>
    ''' <see cref="Open"/> file with readonly parameter set to TRUE
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="retryOpen"></param>
    ''' <param name="verbose"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function OpenReadonly(path As String,
                                 Optional retryOpen As Integer = -1,
                                 Optional verbose As Boolean = False) As Stream
        If retryOpen > 0 Then
            Dim err As Exception = Nothing

            For i As Integer = 0 To retryOpen
                Try
                    Return path.Open(
                        mode:=FileMode.Open,
                        doClear:=False,
                        [readOnly]:=True,
                        verbose:=verbose
                    )
                Catch ex As Exception
                    err = ex
                End Try

                Call Thread.Sleep(100)
            Next

            Throw err
        Else
            Return path.Open(
                mode:=FileMode.Open,
                doClear:=False,
                [readOnly]:=True,
                verbose:=verbose
            )
        End If
    End Function

    Public Const size_2GB As Long = 2 * ByteSize.GB

    ''' <summary>
    ''' Safe open a local file handle. Warning: this function is set to write mode by default, 
    ''' if want using for read file, set <paramref name="doClear"/> to false!
    ''' (打开本地文件指针，这是一个安全的函数，会自动创建不存在的文件夹。这个函数默认是写模式的)
    ''' </summary>
    ''' <param name="path">文件的路径</param>
    ''' <param name="mode">File open mode, default is create a new file.(文件指针的打开模式)</param>
    ''' <param name="doClear">
    ''' By default is preserve all of the data in source file. Which means it is open for write 
    ''' new file data by default. If want to append data or read file, set this argument to false.
    ''' (写模式下默认将原来的文件数据清空)
    ''' 是否将原来的文件之中的数据清空？默认不是，否则将会以追加模式工作
    ''' </param>
    ''' <param name="aggressive">
    ''' memory usage in aggressive mode? default config true means the function will try to 
    ''' load all file data into memory when memory load config is max andalso if the file 
    ''' size is greater than 2GB threshold.
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数只有在完全处于<see cref="FileMode.Open"/>模式下，并且readonly为TRUE，这个时候才会有可能将所有原始数据一次性读取进入内存中
    ''' </remarks>
    <Extension>
    Public Function Open(path$,
                         Optional mode As FileMode = FileMode.OpenOrCreate,
                         Optional doClear As Boolean = False,
                         Optional [readOnly] As Boolean = False,
                         Optional verbose As Boolean = True,
                         Optional aggressive As Boolean = True) As Stream

        Dim shares As FileShare
        Dim access As FileAccess = If([readOnly], FileAccess.Read, FileAccess.ReadWrite)

        If path.StringEmpty Then
            Throw New InvalidProgramException("No file path data provided!")
        ElseIf path.FileName.Length > 200 Then
            Throw New PathTooLongException(path)
        End If

        If mode <> FileMode.Open Then
            With path.ParentPath
                If Not .DirectoryExists Then
                    Call .MakeDir()
                End If
            End With
        End If

        If doClear Then
            ' 在这里调用FlushStream函数的话会导致一个循环引用的问题
            ClearFileBytes(path)
            ' 为了保证数据不被破坏，写操作会锁文件
            shares = FileShare.None
        Else
            ' 读操作，则只允许共享读文件
            shares = FileShare.Read
        End If

        If mode = FileMode.Open AndAlso
            [readOnly] = True AndAlso
            App.MemoryLoad > My.FrameworkInternal.MemoryLoads.Light Then

            Dim file_size As Long = path.FileLength

            If file_size < 0 Then
                Throw New InvalidDataException($"missing raw data file({path}, fullpath={path.GetFullPath(False)}) to read!")
            End If

            ' should reads all data into memory!
            If file_size < size_2GB Then
                If verbose Then
                    Call VBDebugger.EchoLine($"read all binary data into memory for max performance! (size={StringFormats.Lanudry(path.FileLength)}) {path}")
                End If

                ' use a single memorystream object when file size 
                ' is smaller than 2GB
                Return New MemoryStream(path.ReadBinary)
            ElseIf aggressive AndAlso App.MemoryLoad = My.FrameworkInternal.MemoryLoads.Max Then
                ' 20221101
                '
                ' use a memorystream pool object when the file size
                ' is greater than 2GB
                ' default buffer size is 1GB
                Return MemoryStreamPool.FromFile(path)
            End If
        End If

        ' light memory usage
        Return New FileStream(path, mode, access, shares, App.BufferSize)
    End Function

    ''' <summary>
    ''' 将文件之中的所有数据都清空
    ''' </summary>
    ''' <param name="path"></param>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub ClearFileBytes(path As String)
        Call IO.File.WriteAllBytes(path, New Byte() {})
    End Sub

    ''' <summary>
    ''' Open a text file and returns its file handle.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="encoding">使用系统默认的编码方案</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <ExportAPI("Open.Reader")>
    <Extension>
    Public Function OpenReader(path$, Optional encoding As Encoding = Nothing) As StreamReader
        Return New StreamReader(IO.File.Open(path, FileMode.OpenOrCreate), encoding Or UTF8)
    End Function

    ''' <summary>
    ''' <see cref="File.ReadAllBytes"/>, if the file is not exists
    ''' on the filesystem, then a empty array will be return.
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns>
    ''' this method can not read data file when its size is greater 
    ''' than or equals to 2GB size.
    ''' </returns>
    <Extension>
    Public Function ReadBinary(path As String) As Byte()
        If Not path.FileExists Then
            Return {}
        End If

        ' 20220922
        '
        ' this function call may not shared file access
        ' it is unwanted on read common library file
        ' when run parallel, for each process read the
        ' same library data file
        '
        ' Return IO.File.ReadAllBytes(path)
        Using file As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, App.BufferSize)
            Dim buffer_size As Long = file.Length
            Dim buffer As Byte()

            If buffer_size >= size_2GB Then
                Throw New InvalidProgramException($"can not read all binary into memory: the file size({StringFormats.Lanudry(buffer_size)}) of target file '{path}' is greater than 2GB!")
            Else
                buffer = New Byte(buffer_size - 1) {}
            End If

            Call file.Read(buffer, Scan0, buffer_size)

            Return buffer
        End Using
    End Function

    ''' <summary>
    ''' Write all object into a text file by using its <see cref="Object.ToString"/> method.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="saveTo"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function FlushAllLines(Of T)(data As IEnumerable(Of T), saveTo$, Optional encoding As Encodings = Encodings.Default) As Boolean
        Return data.FlushAllLines(saveTo, encoding.CodePage)
    End Function

    ''' <summary>
    ''' Save the binary data into the filesystem.
    ''' </summary>
    ''' <param name="buf">The binary bytes data of the target package's data.(目标二进制数据)</param>
    ''' <param name="path">The saved file path of the target binary data chunk.(目标二进制数据包所要进行保存的文件名路径)</param>
    ''' <returns></returns>
    ''' <remarks>this function will truncates the target file and then save binary data into the file.
    ''' (保存二进制数据包值文件系统)</remarks>
    '''
    <ExportAPI("FlushStream")>
    <Extension>
    Public Function FlushStream(buf As IEnumerable(Of Byte), path$) As Boolean
        ' make target file truncated
        Using write As New BinaryWriter(path.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
            If TypeOf buf Is Byte() Then
                Call write.Write(DirectCast(buf, Byte()))
            Else
                For Each block As Byte() In buf.SplitIterator(partitionSize:=4096)
                    Call write.Write(block)
                Next
            End If

            Call write.Flush()
        End Using

        Return True
    End Function

    <ExportAPI("FlushStream")>
    <Extension>
    Public Function FlushStream(stream As ISerializable, savePath$) As Boolean
        Dim rawStream As Byte() = stream.Serialize
        If rawStream Is Nothing Then
            rawStream = New Byte() {}
        End If
        Return rawStream.FlushStream(savePath)
    End Function
End Module
