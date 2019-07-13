#Region "Microsoft.VisualBasic::9f815b3bcc41dc30d2256a7bfa34ae57, Data\DataFrame\StorageProvider\Reflection\Attributes\Collection.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CollectionAttribute
    ' 
    '         Properties: Delimiter, ProviderId, TypeInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+3 Overloads) CreateObject, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.Reflection

    ''' <summary>
    ''' This property is a array data type object.(并不建议使用本Csv属性来储存大量的文本字符串，极容易出错)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=False)>
    Public Class CollectionAttribute : Inherits ColumnAttribute
        Implements IAttributeComponent

        Public ReadOnly Property Delimiter As String

        Public Overrides ReadOnly Property ProviderId As ProviderIds Implements IAttributeComponent.ProviderId
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ProviderIds.CollectionColumn
            End Get
        End Property

        Protected Friend Shared Shadows ReadOnly Property TypeInfo As Type = GetType(CollectionAttribute)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="delimiter">
        ''' 由于受正则表达式的解析速度的影响，因为CSV文件是使用逗号进行分隔的，假若使用逗号的话，正则表达式的解析速度会比较低，故在这里优先考虑使用分号来作为分隔符
        ''' </param>
        Sub New(name$, Optional delimiter$ = "; ")
            Call MyBase.New(name)
            Me.Delimiter = delimiter
        End Sub

        Public Overrides Function ToString() As String
            Return Me.Name
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CreateObject(cellData As String) As String()
            Return cellData.StringSplit(Delimiter & "\s*")
        End Function

        ''' <summary>
        ''' Collection of object into a cell string content.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function CreateObject(Of T)(source As IEnumerable(Of T)) As String
            Return CreateObject(source, Delimiter)
        End Function

        Public Shared Function CreateObject(Of T)(source As IEnumerable(Of T), delimiter$) As String
            If source Is Nothing Then
                Return ""
            End If

            Dim values$() = LinqAPI.Exec(Of String) _
 _
                () <= From x As T
                      In source
                      Where Not x Is Nothing
                      Select _create = x.ToString

            Return String.Join(delimiter, values)
        End Function
    End Class
End Namespace
