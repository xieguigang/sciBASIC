#Region "Microsoft.VisualBasic::ed5ba19c67abf11edf0e3844aef51abf, Microsoft.VisualBasic.Core\ComponentModel\ValuePair\TagData\LineValue.vb"

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

    '     Structure LineValue
    ' 
    '         Properties: line, value
    ' 
    '         Function: ToString
    ' 
    '         Sub: Assign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.TagData

    Public Structure LineValue(Of T)
        Implements IAddress(Of Integer)
        Implements Value(Of T).IValueOf

        <XmlAttribute>
        Public Property line As Integer Implements IAddress(Of Integer).Address
        Public Property value As T Implements Value(Of T).IValueOf.Value

        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            line = address
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{line}] {Scripting.ToString(value)}"
        End Function
    End Structure
End Namespace
