#Region "Microsoft.VisualBasic::9f46d6f246faad453f2660ccb2a195cb, mime\text%yaml\1.2\Syntax\YamlDocument.vb"

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

    '     Class YamlDocument
    ' 
    ' 
    ' 
    '     Class DataItem
    ' 
    ' 
    ' 
    '     Class Mapping
    ' 
    '         Function: GetMaps
    ' 
    '     Class Scalar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Sequence
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Syntax

    Public Class YamlDocument

        Public Root As DataItem

        Public Directives As New List(Of Directive)()
        Public AnchoredItems As New Dictionary(Of String, DataItem)()

    End Class

    Public Class DataItem

        Public [Property] As NodeProperty

    End Class

    ''' <summary>
    ''' 值是一个字典
    ''' </summary>
    Public Class Mapping : Inherits DataItem

        Public Enties As New List(Of MappingEntry)()

        Public Function GetMaps() As Dictionary(Of MappingEntry)
            Return New Dictionary(Of MappingEntry)(Enties)
        End Function
    End Class

    ''' <summary>
    ''' 值是一个字符串
    ''' </summary>
    Public Class Scalar : Inherits DataItem

        Public Text As String

        Public Sub New()
            Me.Text = String.Empty
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    ''' <summary>
    ''' 值是一个数据序列
    ''' </summary>
    Public Class Sequence
        Inherits DataItem

        Public Enties As New List(Of DataItem)()

    End Class
End Namespace
