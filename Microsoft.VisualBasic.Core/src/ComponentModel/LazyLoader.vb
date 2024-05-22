#Region "Microsoft.VisualBasic::5f2492c0796ad85ab891091031fad3f0, Microsoft.VisualBasic.Core\src\ComponentModel\LazyLoader.vb"

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

    '   Total Lines: 151
    '    Code Lines: 85 (56.29%)
    ' Comment Lines: 43 (28.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 23 (15.23%)
    '     File Size: 4.85 KB


    '     Class LazyLoader
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Properties: URL, Value
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '             Function: ToString, WriteData
    ' 
    '             Sub: __loadData, __printMSG
    ' 
    '     Class Lazy
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel

    ''' <summary>
    ''' 当所需要进行加载的数据的量非常大的时候，则可以使用本方法进行延时按需加载
    ''' </summary>
    ''' <typeparam name="TOutput"></typeparam>
    ''' <remarks></remarks>
    Public Class LazyLoader(Of TOutput As Class, TSource)

        Public Delegate Function DataLoadMethod(source As TSource) As TOutput
        Public Delegate Function DataWriteMethod(source As TSource, obj As TOutput) As Boolean

        ''' <summary>
        ''' Gets the value from the data source <see cref="URL"></see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Value As TOutput
            Get
                If _innerData Is Nothing Then
                    Call __loadData()
                End If

                Return _innerData
            End Get
            Set(value As TOutput)
                _innerData = value
            End Set
        End Property

        Const DATA_LOADED_MESSAGE As String = "[LATE_LOADER_MSG]  {0} data load done!   //{1}; ({2})   ........{3}ms."

        Dim _url As TSource
        Dim _methodInfo As DataLoadMethod
        Dim _innerData As TOutput

        Private Sub __loadData()
            Dim sw As Stopwatch = Stopwatch.StartNew
            _innerData = _methodInfo(_url)
            Call __printMSG(sw.ElapsedMilliseconds)
        End Sub

        Private Sub __printMSG(ElapsedMilliseconds As Long)
            Dim url As String = Me.URL.ToString.ToFileURL
            Dim method As String = _methodInfo.ToString
            Dim msg As String =
            String.Format(DATA_LOADED_MESSAGE, url, _innerData.GetType.FullName, method, ElapsedMilliseconds)

            Call msg.__DEBUG_ECHO
        End Sub

        ''' <summary>
        ''' The data source.(数据源)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property URL As TSource
            Get
                Return _url
            End Get
            Set(value As TSource)
                _url = value
                _innerData = Nothing
            End Set
        End Property

        Sub New(url As TSource, p As DataLoadMethod)
            _url = url
            _methodInfo = p
        End Sub

        Public Overrides Function ToString() As String
            Return _url.ToString.ToFileURL
        End Function

        ''' <summary>
        ''' Write the data back onto the filesystem.(将数据回写进入文件系统之中)
        ''' </summary>
        ''' <param name="WriteMethod"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function WriteData(WriteMethod As DataWriteMethod) As Boolean
            Return WriteMethod(URL, _innerData)
        End Function

        Public Shared Narrowing Operator CType(obj As LazyLoader(Of TOutput, TSource)) As TOutput
            Return obj.Value
        End Operator
    End Class

    ''' <summary>
    ''' The layze loader.
    ''' </summary>
    ''' <typeparam name="TOut"></typeparam>
    Public Class Lazy(Of TOut)

        ''' <summary>
        ''' the data source handler.
        ''' </summary>
        Protected __factory As Func(Of TOut)
        ''' <summary>
        ''' The output result cache data.
        ''' </summary>
        Protected _cache As TOut

        ''' <summary>
        ''' Get cache data if it exists, or the data will be loaded first.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Value As TOut
            Get
                If _cache Is Nothing Then
                    _cache = __factory()
                End If

                Return _cache
            End Get
        End Property

        Sub New(value As TOut)
            _cache = value
        End Sub

        ''' <summary>
        ''' Init this lazy loader with the data source handler.
        ''' </summary>
        ''' <param name="valueFactory">
        ''' The data source provider handler.
        ''' </param>
        Sub New(valueFactory As Func(Of TOut))
            __factory = valueFactory
        End Sub

        Public Overrides Function ToString() As String
            If _cache Is Nothing Then
                Return GetType(TOut).FullName
            Else
                Return _cache.GetJson
            End If
        End Function

        Public Shared Narrowing Operator CType(lazy As Lazy(Of TOut)) As TOut
            Return lazy.Value
        End Operator
    End Class
End Namespace
