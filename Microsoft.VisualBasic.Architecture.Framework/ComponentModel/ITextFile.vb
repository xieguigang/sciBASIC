Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ComponentModel

    ''' <summary>
    ''' Object model of the text file doucment.(文本文件的对象模型，这个文本文件对象在Disposed的时候会自动保存其中的数据)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <PackageNamespace("TextFile", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@gmail.com")>
    Public MustInherit Class ITextFile : Implements System.IDisposable
        Implements I_FileSaveHandle
#If NET_40 = 0 Then
        Implements Microsoft.VisualBasic.ComponentModel.Settings.IProfile
#End If

        ''' <summary>
        ''' This is a file object which have a handle to save its data to the filesystem.(这是一个带有文件数据保存方法的文件模型)
        ''' </summary>
        ''' <remarks></remarks>
        Public Interface I_FileSaveHandle

            ''' <summary>
            ''' Handle for saving the file data.(保存文件的方法)
            ''' </summary>
            ''' <param name="Path">The file path that will save data to.(进行文件数据保存的文件路径)</param>
            ''' <param name="encoding">The text encoding value for the text document.(文本文档的编码格式)</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Function Save(Optional Path As String = "", Optional encoding As System.Text.Encoding = Nothing) As Boolean
        End Interface

        Protected _FilePath As String

        ''' <summary>
        ''' The source file.(源文件)
        ''' </summary>
        ''' <remarks></remarks>
        Protected Friend _innerBuffer As String()

        ''' <summary>
        ''' The storage filepath of this text file.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
#If NET_40 = 0 Then
        <Xml.Serialization.XmlIgnore>
        Public Overridable Property FilePath As String Implements Settings.IProfile.FilePath
#Else
        <Xml.Serialization.XmlIgnore>
        Public Overridable Property FilePath As String
#End If
            Get
                If String.IsNullOrEmpty(_FilePath) Then
                    Return ""
                End If
                Return FileIO.FileSystem.GetFileInfo(_FilePath).FullName.Replace("\", "/")
            End Get
            Set(value As String)
                _FilePath = value
            End Set
        End Property

#If NET_40 = 0 Then
        Public MustOverride Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean Implements Settings.IProfile.Save, I_FileSaveHandle.Save
#Else
        Public MustOverride Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean Implements I_FileSaveHandle.Save
#End If

        Public Overrides Function ToString() As String
            Dim Path As String = FilePath
            If String.IsNullOrEmpty(Path) Then
                Return MyBase.ToString
            Else
                Return Path.ToFileURL
            End If
        End Function

        Protected Friend Sub CopyTo(Of T As Microsoft.VisualBasic.ComponentModel.ITextFile)(ByRef TextFile As T)
            TextFile._innerBuffer = Me._innerBuffer
            TextFile._FilePath = Me._FilePath
        End Sub

        ''' <summary>
        ''' Automatically determine the path paramater: If the target path is empty, then return
        ''' the file object path <see cref="FilePath"></see> property, if not then return the
        ''' <paramref name="path"></paramref> directly.
        ''' (当<paramref name="path"></paramref>的值不为空的时候，本对象之中的路径参数将会被替换，反之返回本对象的路径参数)
        ''' </summary>
        ''' <param name="path">用户所输入的文件路径</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overridable Function getPath(path As String) As String
            If String.IsNullOrEmpty(path) Then
                path = FilePath
            Else
                FilePath = path
            End If

            If String.IsNullOrEmpty(path) Then
                FilePath = __getDefaultPath()
                Return FilePath
            End If

            Return path
        End Function

        Protected Overridable Function __getDefaultPath() As String
            Return ""
        End Function

        Protected Shared Function getEncoding(encoding As System.Text.Encoding) As System.Text.Encoding
            If encoding Is Nothing Then
                Return System.Text.Encoding.Default
            Else
                Return encoding
            End If
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding">Default value is UTF8</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Read.TXT")>
        Public Shared Function Read(FilePath As String, Optional Encoding As System.Text.Encoding = Nothing) As String
            If Encoding Is Nothing Then
                Encoding = System.Text.Encoding.UTF8
            End If
            Return FileIO.FileSystem.ReadAllText(FilePath, encoding:=Encoding)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="FilePath"></param>
        ''' <param name="Encoding">Default value is UTF8</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Read.Lines")>
        Public Shared Function ReadAllLines(FilePath As String, Optional Encoding As System.Text.Encoding = Nothing) As String()
            If Encoding Is Nothing Then
                Encoding = System.Text.Encoding.UTF8
            End If
            Return IO.File.ReadAllLines(FilePath, encoding:=Encoding)
        End Function

        Public Shared Narrowing Operator CType(file As ITextFile) As String
            Return String.Join(vbLf, file._innerBuffer)
        End Operator

        Public Shared Widening Operator CType(file As ITextFile) As String()
            Return file._innerBuffer
        End Operator

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call Save()
                    ' TODO:  释放托管状态(托管对象)。
                End If

                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                ' TODO:  将大型字段设置为 null。
            End If
            Me.disposedValue = True
        End Sub

        ' TODO:  仅当上面的 Dispose(      disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose(      disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码是为了正确实现可处置模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Interface IDocumentEditor : Inherits I_FileSaveHandle
            Property DocumentPath As String
            Function LoadDocument(Path As String) As Boolean
        End Interface

        '''' <summary>
        '''' Read the data in the target database file.
        '''' (从目标文件中读取数据)
        '''' </summary>
        '''' <param name="pathOrTextContent">The file path of the target database file.(目标数据库文件的文件路径)</param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Shared Widening Operator CType(pathOrTextContent As String) As ITextFile
        '    Return ITextFile.CreateObject(Of ITextFile)
        'End Operator

        Public Shared Function CreateObject(Of T As ITextFile)(innerData As String(), path As String) As T
            Dim File As T = Activator.CreateInstance(Of T)
            File._FilePath = path
            File._innerBuffer = innerData

            Return File
        End Function

        Public Shared Function CreateObject(Of T As ITextFile)(pathOrTextContent As String) As T
            If FileIO.FileSystem.FileExists(pathOrTextContent) Then
                Try
                    Dim FileObject As T = ITextFile.CreateObject(Of T)(System.IO.File.ReadAllLines(pathOrTextContent), path:=pathOrTextContent) 'New ITextFile With {.InternalFileData = , ._FilePath = Path}
                    Return FileObject
                Catch ex As Exception
                    GoTo NULL
                End Try
            End If
NULL:
            Return ITextFile.CreateObject(Of T)({pathOrTextContent}, "") ' 字符串参数可能是文档的内容
        End Function

        <ExportAPI("Text.Partition")>
        Public Shared Function TextPartition(data As Generic.IEnumerable(Of String)) As String()()
            Dim maxSize As Double = New StringBuilder(1024 * 1024).MaxCapacity
            Return __textPartitioning(data.ToArray, maxSize)
        End Function

        Private Shared Function __textPartitioning(dat As String(), maxSize As Double) As String()()
            Dim currentSize As Double = (From s As String In dat.AsParallel Select CDbl(Len(s))).ToArray.Sum
            If currentSize > maxSize Then
                Dim splits = dat.Split(dat.Length / 2)
                If splits.Length > 1 Then
                    Return (From n In splits Select __textPartitioning(n, maxSize)).ToArray.MatrixToVector
                Else
                    Return splits
                End If
            Else
                Return New String()() {dat}
            End If
        End Function

        ''' <summary>
        ''' 当一个文件非常大以致无法使用任何现有的文本编辑器查看的时候，可以使用本方法查看其中的一部分数据 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Peeks")>
        Public Shared Function Peeks(path As String, length As Integer) As String
            Return Microsoft.VisualBasic.Peeks(path, length)
        End Function

        ''' <summary>
        ''' 尝试查看大文件的尾部的数据
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Tails")>
        Public Shared Function Tails(path As String, length As Integer) As String
            Return Microsoft.VisualBasic.Tails(path, length)
        End Function

    End Class
End Namespace