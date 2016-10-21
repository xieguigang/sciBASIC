Imports System.Runtime.Serialization
Imports System.Web.Script.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

''' <summary>
''' 匹配结果
''' </summary>
''' 
<KnownType(GetType(NamedValue(Of String)))>
Public Structure Match

    <ScriptIgnore>
    Public Property x As Object
    Dim score#
    Dim Field As NamedValue(Of String)

    Public ReadOnly Property Success As Boolean
        Get
            Return score > 0R
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Field.ToString
    End Function

    Public Shared Narrowing Operator CType(m As Match) As Boolean
        Return m.Success
    End Operator
End Structure

''' <summary>
''' 调用这个方法计算出匹配结果
''' </summary>
''' <param name="def">数据定义缓存</param>
''' <param name="obj">数据实体</param>
''' <returns></returns>
Public Delegate Function IAssertion(def As IObject, obj As Object) As Match