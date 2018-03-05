#Region "Microsoft.VisualBasic::a8d53bceb674239c349fbecda1c778df, Microsoft.VisualBasic.Core\ComponentModel\File\ITextFile.vb"

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

    '     Class ITextFile
    ' 
    '         Properties: (+2 Overloads) FilePath
    ' 
    '         Function: __getDefaultPath, getEncoding, getPath, Save, ToString
    ' 
    '         Sub: CopyTo, (+2 Overloads) Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel

    ''' <summary>
    ''' Object model of the text file doucment.(文本文件的对象模型，这个文本文件对象在Disposed的时候会自动保存其中的数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class ITextFile : Inherits BaseClass
        Implements IDisposable
        Implements ISaveHandle
        Implements IFileReference
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
        Public Overridable Property FilePath As String Implements Settings.IProfile.FilePath, IFileReference.FilePath
#Else
        <XmlIgnore> <ScriptIgnore>
        Public Overridable Property FilePath As String
#End If

#If NET_40 = 0 Then
        Public MustOverride Function Save(Optional path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements Settings.IProfile.Save, ISaveHandle.Save
#Else
        Public MustOverride Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
#End If

        Public Overrides Function ToString() As String
            Dim path As String = FilePath

            If String.IsNullOrEmpty(path) Then
                Return MyBase.ToString
            Else
                Return path.ToFileURL
            End If
        End Function

        Protected Friend Sub CopyTo(Of T As ITextFile)(ByRef TextFile As T)
            TextFile.Extension = Extension
            TextFile.FilePath = FilePath
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

        Protected Shared Function getEncoding(encoding As Encoding) As Encoding
            If encoding Is Nothing Then
                Return Encoding.Default
            Else
                Return encoding
            End If
        End Function

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' 检测冗余的调用

        ' IDisposable
        Protected Overridable Overloads Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Call Save(encoding:=UTF8WithoutBOM)
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
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Function Save(Optional path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function
    End Class
End Namespace
