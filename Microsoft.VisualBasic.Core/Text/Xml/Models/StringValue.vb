#Region "Microsoft.VisualBasic::e8744a6c59f7de0ab8941bd85c4a625f, Microsoft.VisualBasic.Core\Text\Xml\Models\StringValue.vb"

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

    '     Class StringValue
    ' 
    '         Properties: value
    ' 
    '         Function: ToString
    ' 
    '     Structure LineValue
    ' 
    '         Properties: line, text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace Text.Xml.Models

    Public Class StringValue : Implements Value(Of String).IValueOf

        ''' <summary>
        ''' A short text value without new line symbols
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property value As String Implements Value(Of String).IValueOf.Value

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class

    ''' <summary>
    ''' 代码行的模型？
    ''' </summary>
    Public Structure LineValue

        <XmlAttribute>
        Public Property line As Integer
        <XmlText>
        Public Property text As String

        Public Overrides Function ToString() As String
            Return $"[{line}]  {text}"
        End Function
    End Structure
End Namespace
