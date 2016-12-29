#Region "Microsoft.VisualBasic::378997baf16e14e0a874fa6a635bf928, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\UnixBash\FileSystem\Searchs.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports System.Text.RegularExpressions

Namespace Language.UnixBash

    Public Module FileSystemAPI

        ''' <summary>
        ''' ``ls -l -ext("*.xml") &lt;= DIR``,  The filesystem search proxy
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ls As New Search
        ''' <summary>
        ''' Long name(DIR+fiename), if not only file name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property l As New SearchOpt(SearchOpt.Options.LongName)
        ''' <summary>
        ''' 递归的搜索
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property r As New SearchOpt(SearchOpt.Options.Recursive)
        ''' <summary>
        ''' Searching the directory, if this parameter is not presents, then returns search file.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property lsDIR As New SearchOpt(SearchOpt.Options.Directory)

        ''' <summary>
        ''' 可以使用这个来限定文件或者文件夹对象的搜索范围
        ''' </summary>
        ''' <param name="__wildcards">可以为文件拓展或者对文件名的通配符的表达式，假若这个是空的，则会默认搜索所有文件*.*</param>
        ''' <returns></returns>
        Public Function wildcards(ParamArray __wildcards As String()) As SearchOpt
            Dim opt As New SearchOpt(SearchOpt.Options.Ext)

            If __wildcards.IsNullOrEmpty Then
                __wildcards = {"*.*"}
            End If

            For Each s As String In __wildcards
                Call opt.wildcards.Add(s)
            Next

            Return opt
        End Function

        Public Function DIRHandle(res As String) As Integer
            Return OpenHandle(res)
        End Function
    End Module

    Public Class Search : Implements ICloneable

        Dim __opts As New Dictionary(Of SearchOpt.Options, SearchOpt)

        Public Overrides Function ToString() As String
            Return __opts.GetJson
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Search With {
                .__opts = New Dictionary(Of SearchOpt.Options, SearchOpt)(__opts)
            }
        End Function

        ''' <summary>
        ''' Add a search options
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="l"></param>
        ''' <returns></returns>
        Public Shared Operator -(ls As Search, l As SearchOpt) As Search
            If l.opt <> SearchOpt.Options.None Then
                Dim clone As Search = DirectCast(ls.Clone, Search)
                Call clone.__opts.Add(l.opt, l)
                Return clone
            Else
                Return ls
            End If
        End Operator

        Public Shared Operator -(ls As Search, wildcards$()) As Search
            Return ls - FileSystemAPI.wildcards(wildcards$)
        End Operator

        Public Shared Operator -(ls As Search, wildcards$) As Search
            Return ls - wildcards.Split(","c)
        End Operator

        Public ReadOnly Property SearchType As FileIO.SearchOption
            Get
                Dim opt As FileIO.SearchOption = FileIO.SearchOption.SearchTopLevelOnly
                If __opts.ContainsKey(SearchOpt.Options.Recursive) Then
                    Return FileIO.SearchOption.SearchAllSubDirectories
                Else
                    Return opt
                End If
            End Get
        End Property

        Public ReadOnly Property wildcards As String()
            Get
                If Not __opts.ContainsKey(SearchOpt.Options.Ext) Then
                    Return Nothing
                Else
                    Return __opts(SearchOpt.Options.Ext).wildcards.ToArray
                End If
            End Get
        End Property

        ''' <summary>
        ''' Search the files in the specific directory
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        Public Shared Operator <<(ls As Search, DIR As Integer) As IEnumerable(Of String)
            Dim url As String = __getHandle(DIR).FileName
            Return ls < url
        End Operator

        ''' <summary>
        ''' Search the files in the specific directory
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        Public Shared Operator <(ls As Search, DIR As String) As IEnumerable(Of String)
            Return ls <= DIR
        End Operator

        ''' <summary>
        ''' Search the files in the specific directory
        ''' </summary>
        ''' <param name="ls"></param>
        ''' <param name="DIR"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <=(ls As Search, DIR As String) As IEnumerable(Of String)
            Dim l As Boolean = ls.__opts.ContainsKey(SearchOpt.Options.LongName)
            Dim wc As String() =
                ls.wildcards.ToArray(Function(x) x.Replace(".", "\."))
            For i As Integer = 0 To wc.Length - 1
                If wc(i).Last <> "*"c Then
                    wc(i) = wc(i) & "$"
                End If
                wc(i) = wc(i).Replace("*", ".+")
            Next
            Dim isMatch As Func(Of String, Boolean) =
                AddressOf New wildcardsCompatible With {
                    .regexp = If(wc.Length = 0, {".+"}, wc)
                }.IsMatch

            If ls.__opts.ContainsKey(SearchOpt.Options.Directory) Then
                Dim res As IEnumerable(Of String) =
                    FileIO.FileSystem.GetDirectories(DIR, ls.SearchType)

                If l Then
                    Return res.Where(isMatch)
                Else
                    Return res.Where(isMatch).ToArray(Function(s) s.BaseName)
                End If
            Else
                Dim res As IEnumerable(Of String) =
                    FileIO.FileSystem.GetFiles(DIR, ls.SearchType)

                If l Then
                    Return res.Where(isMatch)
                Else
                    Return From path As String
                           In res
                           Where isMatch(path)
                           Select path.Replace("\", "/").Split("/"c).Last
                End If
            End If
        End Operator

        Public Shared Operator >(ls As Search, DIR As String) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator

        Public Shared Operator >=(ls As Search, DIR As String) As IEnumerable(Of String)
            Throw New NotSupportedException
        End Operator
    End Class

    ''' <summary>
    ''' Using regular expression to find a match on the file name.
    ''' </summary>
    Public Structure wildcardsCompatible

        Dim regexp As String()

        ''' <summary>
        ''' Windows系统上面文件路径不区分大小写，但是Linux、Mac系统却区分大小写
        ''' 所以使用这个来保持对Windows文件系统的兼容性
        ''' </summary>
        Shared ReadOnly opt As RegexOptions =
            If(App.Platform = PlatformID.MacOSX OrElse
            App.Platform = PlatformID.Unix,
            RegexOptions.Singleline,
            RegexICSng)

        ''' <summary>
        ''' Linux/Mac系统不支持Windows系统的通配符，所以在这里是用正则表达式来保持代码的兼容性
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Function IsMatch(path As String) As Boolean
            If regexp.IsNullOrEmpty Then
                ' 匹配的规则是空的，则默认是允许所有的路径
                Return True
            End If

            Dim name As String = path.Replace("\", "/").Split("/"c).Last

            For Each r As String In regexp
                If Regex.Match(name, r, opt).Success Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Structure

    Public Structure SearchOpt

        Dim opt As Options
        Dim value As String
        Dim wildcards As List(Of String)

        Sub New(opt As Options, s As String)
            Me.opt = opt
            Me.value = s
            Me.wildcards = New List(Of String)
        End Sub

        Sub New(opt As Options)
            Call Me.New(opt, "")
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Enum Options
            None
            Ext
            LongName
            ''' <summary>
            ''' List directories, not files listing.
            ''' </summary>
            Directory
            ''' <summary>
            ''' 递归搜索所有的文件夹
            ''' </summary>
            Recursive
        End Enum
    End Structure
End Namespace
