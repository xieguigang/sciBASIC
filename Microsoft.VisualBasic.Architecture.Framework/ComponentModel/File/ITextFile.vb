Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ComponentModel

    ''' <summary>
    ''' Object model of the text file doucment.(文本文件的对象模型，这个文本文件对象在Disposed的时候会自动保存其中的数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class ITextFile : Inherits BufferedStream
        Implements IDisposable
        Implements ISaveHandle
#If NET_40 = 0 Then
        Implements Settings.IProfile
#End If

        ''' <summary>
        ''' The storage filepath of this text file.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
#If NET_40 = 0 Then
        <XmlIgnore> <ScriptIgnore>
        Protected Friend Overridable Property FilePath As String Implements Settings.IProfile.FilePath
#Else
        <XmlIgnore>
        Public Overridable Property FilePath As String
#End If
            Get
                If String.IsNullOrEmpty(FileName) Then
                    Return ""
                End If
                Return FileIO.FileSystem.GetFileInfo(FileName).FullName.Replace("\", "/")
            End Get
            Set(value As String)
                FileName = value
            End Set
        End Property

#If NET_40 = 0 Then
        Public MustOverride Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean Implements Settings.IProfile.Save, ISaveHandle.Save
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

        Protected Friend Sub CopyTo(Of T As ITextFile)(ByRef TextFile As T)
            TextFile.__bufferSize = Me.__bufferSize
            TextFile.__encoding = Me.__encoding
            TextFile.__fileName = Me.FileName
            TextFile.__innerBuffer = Me.__innerBuffer
            TextFile.__innerStream = Me.__innerStream
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

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call Save(Encoding:=Encoding.UTF8)
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

        Public Shared Function CreateObject(Of T As ITextFile)(innerData As String(), path As String) As T
            Dim File As T = Activator.CreateInstance(Of T)
            File.FilePath = path
            File.__innerBuffer = innerData

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

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.GetEncodings)
        End Function
    End Class
End Namespace