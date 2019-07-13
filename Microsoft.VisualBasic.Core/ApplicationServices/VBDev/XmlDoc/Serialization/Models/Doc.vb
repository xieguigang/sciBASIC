#Region "Microsoft.VisualBasic::5d3d1d04a1aa244b9b8318273a109441, Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\Serialization\Models\Doc.vb"

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

    '     Class Doc
    ' 
    '         Properties: assembly, members
    ' 
    '         Function: ToString
    ' 
    '     Class assembly
    ' 
    '         Properties: name
    ' 
    '         Function: ToString
    ' 
    '     Interface IMember
    ' 
    '         Properties: name
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace ApplicationServices.Development.XmlDoc.Serialization

    ''' <summary>
    ''' .NET assembly generated XML comments documents file.
    ''' </summary>
    <XmlType("doc")> Public Class Doc
        Public Property assembly As assembly
        Public Property members As member()

        Public Overrides Function ToString() As String
            Return assembly.name
        End Function
    End Class

    Public Class assembly : Implements IMember

        Public Property name As String Implements IMember.name

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class

    Public Interface IMember
        Property name As String
    End Interface
End Namespace
