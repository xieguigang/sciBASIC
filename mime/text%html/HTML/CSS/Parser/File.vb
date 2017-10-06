Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace HTML.CSS.Parser

    ''' <summary>
    ''' CSS文件的对象模型，一个CSS文件是由若干个selector节点选择器所构成的，以及每一个选择器都是由若干样式属性定义所构成
    ''' </summary>
    Public Class CSSFile

        Public Property Selectors As Dictionary(Of Selector)

        ''' <summary>
        ''' GetElementByID
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByID As Selector()
            Get
                Return GetAllStylesByType(Types.ID)
            End Get
        End Property

        ''' <summary>
        ''' GetElementsByClass
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByClass As Selector()
            Get
                Return GetAllStylesByType(Types.Class)
            End Get
        End Property

        ''' <summary>
        ''' By html tags
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByTag As Selector()
            Get
                Return Selectors _
                    .Where(Function(style)
                               With style.Key.First
                                   If Not .Equals("."c) AndAlso Not .Equals("#"c) Then
                                       Return True
                                   Else
                                       Return False
                                   End If
                               End With
                           End Function) _
                    .Values
            End Get
        End Property

        Default Public ReadOnly Property GetSelector(name$) As Selector
            Get
                If Selectors.ContainsKey(name) Then
                    Return Selectors(name)
                Else
                    ' 因为CSS是手工编写的，所以可能会出现大小写错误的问题
                    ' 如果字典查找失败的话，则尝试使用字符串匹配来查找
                    Return Selectors _
                        .Values _
                        .Where(Function(style)
                                   Return style.Selector.TextEquals(name)
                               End Function) _
                        .FirstOrDefault
                End If
            End Get
        End Property

        ''' <summary>
        ''' 获取某一种类型之下的所有的该类型的CSS的样式定义
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function GetAllStylesByType(type As Types) As Selector()
            Return Selectors.Values _
                .Where(Function(style) style.Type = type) _
                .ToArray
        End Function

        ''' <summary>
        ''' 根据类型来获取得到相应的选择器的样式
        ''' </summary>
        ''' <param name="name$">没有class或者ID的符号前缀的名称</param>
        ''' <param name="type">class还是ID或者还是html的标签名称？</param>
        ''' <returns></returns>
        Public Function FindStyle(name$, type As Types) As Selector
            With ("." & name) Or ("#" & name).AsDefault(Function() type = Types.ID)
                Return GetSelector(.ref)
            End With
        End Function

        Public Overrides Function ToString() As String
            Return Selectors.Keys.ToArray.GetJson
        End Function
    End Class

    ''' <summary>
    ''' CSS之中的样式选择器
    ''' </summary>
    Public Class Selector : Inherits [Property](Of String)
        Implements INamedValue

        ''' <summary>
        ''' 选择器的名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Selector As String Implements IKeyedEntity(Of String).Key
        Public ReadOnly Property Type As Types
            Get
                If Selector.First = "."c Then
                    Return Types.Class
                ElseIf Selector.First = "#" Then
                    Return Types.ID
                Else
                    Return Types.Unknown
                End If
            End Get
        End Property

        ''' <summary>
        ''' CSS style value without selector name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CSSValue As String
            Get
                Return Properties _
                    .Select(Function(x) $"{x.Key}: {x.Value};") _
                    .JoinBy(ASCII.LF)
            End Get
        End Property

        ''' <summary>
        ''' Get the selector text name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            Get
                Return Selector.Trim("#"c, "."c)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Selector & " { " & CSSValue & " }"
        End Function
    End Class

    Public Enum Types As Byte
        Unknown
        ID
        [Class]
    End Enum
End Namespace