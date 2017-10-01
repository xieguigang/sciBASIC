Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace HTML.CSS.Render

    Public Class CSSFile

        Public Property Selectors As Dictionary(Of Selector)

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

        Public Function FindStyle(name$, type As Types) As Selector
            With ("." & name) Or ("#" & name).AsDefault(Function() type = Types.ID)
                Return GetSelector(.ref)
            End With
        End Function
    End Class

    Public Class Selector : Inherits [Property](Of String)
        Implements INamedValue

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
        ''' Get the selector text name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
            Get
                Return Selector.Trim("#"c, "."c)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Selector & "{" &
                Properties _
                    .Select(Function(x) $"{x.Key}: {x.Value};") _
                    .JoinBy(ASCII.LF) &
            "}"
        End Function
    End Class

    Public Enum Types As Byte
        Unknown
        ID
        [Class]
    End Enum
End Namespace