#Region "Microsoft.VisualBasic::d7e455ac86ff34a1148c3e992326fcb8, Microsoft.VisualBasic.Core\src\Net\HTTP\Web\WebQuery.vb"

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

    '   Total Lines: 352
    '    Code Lines: 196 (55.68%)
    ' Comment Lines: 105 (29.83%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 51 (14.49%)
    '     File Size: 14.81 KB


    '     Class WebQuery
    ' 
    '         Properties: offlineMode
    ' 
    '         Constructor: (+5 Overloads) Sub New
    ' 
    '         Function: GetText, IsNullKey, (+2 Overloads) Query, QueryCacheText, queryText
    '                   (+2 Overloads) queryTextImpl
    ' 
    '         Sub: Clear404URLIndex, (+2 Overloads) Dispose, runHttpGet, Write404CacheList
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Perl
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Net.Http

    ''' <summary>
    ''' <typeparamref name="Context"/>类型参数应该是查询的term的数据类型, 而非返回的查询结果的数据类型
    ''' </summary>
    ''' <typeparam name="Context"></typeparam>
    ''' <remarks>
    ''' 这个模块不会重复请求404状态的资源
    ''' </remarks>
    ''' 
    <FrameworkConfig(WebQuery(Of Boolean).WebQueryDebug)>
    Public Class WebQuery(Of Context) : Implements IDisposable, IHttpGet

        Friend url As Func(Of Context, String)
        Friend contextGuid As IToString(Of Context)
        Friend deserialization As IObjectBuilder
        Friend prefix As Func(Of String, String)

        ''' <summary>
        ''' 404状态的资源列表
        ''' </summary>
        Dim url404 As New Index(Of String)

        ''' <summary>
        ''' 是否是处于仅从缓存数据之中查找结果的离线模式
        ''' </summary>
        ''' <returns></returns>
        Public Property offlineMode As Boolean

        ''' <summary>
        ''' 原始请求结果数据的缓存文件夹,同时也可以用这个文件夹来存放错误日志
        ''' </summary>
        Protected cache As IFileSystemEnvironment
        Protected sleepInterval As Integer

        Protected Shared debug As Boolean = True
        Private disposedValue As Boolean

        Shared ReadOnly interval As [Default](Of Integer)

        Friend Const WebQueryDebug As String = "webquery.debug"

        Shared Sub New()
            Static defaultInterval As [Default](Of String) = "3000"

            With Val(App.GetVariable("sleep") Or defaultInterval)
                ' controls of the interval by /@set sleep=xxxxx
                interval = CInt(.ByRef).AsDefault(Function(x) x <= 0)
                debug = App.GetVariable(WebQueryDebug).ParseBoolean
            End With

            If debug Then
                ' display debug info
                Call $"WebQuery download worker for query context [{GetType(Context).FullName}] thread sleep interval is {interval}ms".__INFO_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url">请注意,查询词应该是被<see cref="UrlEncode"/>所转义过的</param>
        ''' <param name="contextGuid"></param>
        ''' <param name="parser"></param>
        ''' <param name="prefix">
        ''' 如果查询的结果文件很多, 则缓存放在同一下文件夹下, 打开的效率会非常低,
        ''' 在这里使用这个函数来得到分组前缀用作为文件夹名,分组存放缓存数据
        ''' </param>
        ''' <param name="cache">
        ''' A local <see cref="Directory"/> will be open for implements the 
        ''' <see cref="IFileSystemEnvironment"/> wrapper as cache.
        ''' </param>
        ''' <param name="interval"></param>
        Sub New(url As Func(Of Context, String),
                Optional contextGuid As IToString(Of Context) = Nothing,
                Optional parser As IObjectBuilder = Nothing,
                Optional prefix As Func(Of String, String) = Nothing,
                <CallerMemberName>
                Optional cache$ = Nothing,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call Me.New(cache, interval, offline)

            Me.url = url
            Me.contextGuid = contextGuid Or Scripting.ToString(Of Context)
            Me.deserialization = parser Or XmlParser
            Me.prefix = prefix
        End Sub

        Sub New(url As Func(Of Context, String), cache As IFileSystemEnvironment,
                Optional contextGuid As IToString(Of Context) = Nothing,
                Optional parser As IObjectBuilder = Nothing,
                Optional prefix As Func(Of String, String) = Nothing,
                Optional interval% = -1,
                Optional offline As Boolean = False)

            Call Me.New(cache, interval, offline)

            Me.url = url
            Me.contextGuid = contextGuid Or Scripting.ToString(Of Context)
            Me.deserialization = parser Or XmlParser
            Me.prefix = prefix
        End Sub

        ''' <summary>
        ''' create workspace from a local filesystem
        ''' </summary>
        ''' <param name="cache"></param>
        ''' <param name="interval"></param>
        ''' <param name="offline"></param>
        Friend Sub New(cache$, interval%, offline As Boolean)
            Call Me.New(New Directory(cache), interval, offline)
        End Sub

        Friend Sub New(cache As IFileSystemEnvironment, interval%, offline As Boolean)
            Me.cache = cache
            Me.sleepInterval = interval Or WebQuery(Of Context).interval
            Me.offlineMode = offline

            If offlineMode AndAlso debug Then
                Call $"WebQuery of '{Me.GetType.Name}' running in offline mode!".__DEBUG_ECHO
            End If

            Me.url404 = cache.ReadAllText("/__404.txt").LineTokens.Indexing
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Write404CacheList()
            Call cache.WriteText(text:=url404.Objects.JoinBy(vbCrLf), path:="/__404.txt")
            Call cache.Flush()
        End Sub

        Public Sub Clear404URLIndex()
            Call url404.Clear()
            Call Write404CacheList()
        End Sub

        ''' <summary>
        ''' test if the given key is empty or nothing
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function IsNullKey(key As Object) As Boolean
            Return ExceptionHandle.Default(key)
        End Function

        ''' <summary>
        ''' 这个函数返回的是缓存的本地文件的路径列表
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="type">文件拓展名，可以不带有小数点</param>
        ''' <returns></returns>
        Protected Iterator Function queryText(query As IEnumerable(Of Context), type$) As IEnumerable(Of (cache$, hitCache As Boolean))
            ' 因为在这里是进行批量的数据库查询
            ' 所以在这个函数内的代码的执行效率不会被考虑在内
            For Each context As Context In query
                If IsNullKey(context) Then
                    Yield ("", True)
                Else
                    Yield queryTextImpl(context, type)
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function queryTextImpl(context As Context, type As String) As (cache$, hitCache As Boolean)
            Return queryTextImpl(id:=contextGuid(context), url:=url(context), type)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="id"></param>
        ''' <param name="url"></param>
        ''' <param name="type">the file extension name, for indicate the file type, example as: *.txt</param>
        ''' <returns></returns>
        Private Function queryTextImpl(id As String, url As String, type As String) As (cache$, hitCache As Boolean)
            ' the cache path
            Dim cache_file As String
            ' 如果是进行一些分子名称的查询,可能会因为分子名称超长而导致文件系统api调用出错
            ' 所以在这里需要截短一下文件名称
            ' 因为路径的总长度不能超过260个字符,所以文件名这里截短到200字符以内,留给文件夹名称一些长度
            Dim baseName$ = Mid(id, 1, 192)
            Dim hitCache As Boolean = True

            If prefix Is Nothing Then
                cache_file = $"/{baseName}.{type.Trim("."c, "*"c)}"
            Else
                cache_file = $"/{prefix(id)}/{baseName}.{type.Trim("."c, "*"c)}"
            End If

            If Not url Like url404 Then
                Call runHttpGet(cache_file, url, hitCache)
            ElseIf debug Then
                Call $"{id} 404 Not Found!".PrintException
            End If

            'If TypeOf Me.cache Is Directory Then
            '    cache_file = $"{DirectCast(cache, Directory).folder}/{cache_file}"
            'End If

            Return (cache_file, hitCache)
        End Function

        Private Function GetText(url As String) As String Implements IHttpGet.GetText
            Dim q = queryTextImpl(id:=url.MD5, url, type:="*.txt")
            Dim cache As String = Me.cache.ReadAllText(q.cache)
            Return cache
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="cache_path">a relative cache path</param>
        ''' <param name="url$"></param>
        ''' <param name="hitCache"></param>
        Private Sub runHttpGet(cache_path As String, url$, ByRef hitCache As Boolean)
            Dim is404 As Boolean = False
            Dim is_missing As Boolean = cache.FileSize(cache_path) <= 0
            Dim is_empty As Boolean = True

            If Not is_missing Then
                Dim debug_text As String = cache.ReadAllText(cache_path) _
                    .TrimNewLine _
                    .Trim _
                    .DoCall(AddressOf Strings.Trim)

                is_empty = debug_text.StringEmpty
            End If

            If (is_missing OrElse is_empty) AndAlso Not offlineMode Then
                Call cache.WriteText(url.GET(is404:=is404), cache_path)
                Call cache.Flush()
                Call Thread.Sleep(sleepInterval)

                ' andalso treated the empty web response as 
                ' the 404 error as well?
                If is404 OrElse cache.FileSize(cache_path) <= 8 Then
                    url404 += url

                    Call Write404CacheList()
                    Call $"{url} 404 Not Found!".PrintException
                ElseIf debug Then
                    Call $"Worker thread sleep {sleepInterval}ms...".__INFO_ECHO
                End If

                hitCache = False
            ElseIf debug Then
                Call "hit cache!".__DEBUG_ECHO
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="context"></param>
        ''' <param name="cacheType">缓存文件的文本格式拓展名</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Query(Of T)(context As Context, Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As T
            Return QueryCacheText(context, cacheType, hitCache).DoCall(Function(text) deserialization(text, GetType(T)))
        End Function

        Public Function QueryCacheText(context As Context, Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As String
            hitCache = False

            If context Is Nothing Then
                Return Nothing
            Else
                Dim result As (cache$, hitCache As Boolean) = queryText({context}, cacheType).First
                Dim cache As String = Me.cache.ReadAllText(result.cache)

                hitCache = result.hitCache
                ' get cache text data
                Return cache
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="context"></param>
        ''' <param name="cacheType">缓存文件的文本格式拓展名</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Query(Of T)(context As IEnumerable(Of Context), Optional cacheType$ = ".xml", Optional ByRef hitCache As Boolean = False) As IEnumerable(Of T)
            Dim result = queryText(context, cacheType).ToArray
            Dim hits = result.Where(Function(pop) pop.hitCache).Count

            hitCache = (hits / result.Length) > 0.6

            Return result _
                .Select(Function(file)
                            Return file.cache _
                                .ReadAllText(throwEx:=False) _
                                .DoCall(Function(text)
                                            Return deserialization(text, GetType(T))
                                        End Function)
                        End Function) _
                .As(Of T)
        End Function

        ''' <summary>
        ''' Release and save the cache filesystem data handle
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    If Not cache Is Nothing Then
                        Call cache.Close()
                    End If
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
