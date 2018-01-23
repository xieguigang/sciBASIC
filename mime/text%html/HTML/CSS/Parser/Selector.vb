Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Text

Namespace HTML.CSS.Parser

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

        Public ReadOnly Property Type As CSSSelectorTypes
            Get
                If Selector.First = "."c Then
                    Return CSSSelectorTypes.class
                ElseIf Selector.First = "#" Then
                    Return CSSSelectorTypes.id
                ElseIf Selector.GetTagValue <> HtmlTags.NA Then
                    Return CSSSelectorTypes.tag
                Else
                    Return CSSSelectorTypes.expression
                End If
            End Get
        End Property

        ''' <summary>
        ''' CSS style value without selector name.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CSSValue As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Selector.Trim("#"c, "."c)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Selector & " { " & CSSValue & " }"
        End Function
    End Class
End Namespace