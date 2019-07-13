#Region "Microsoft.VisualBasic::e45ba7a7b38d087fda2074ee05a6a4ec, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IXml.vb"

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

    ' Class IXml
    ' 
    '     Function: ToString, WriteXml
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Public MustInherit Class IXml

    Protected MustOverride Function filePath() As String
    Protected MustOverride Function toXml() As String

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return filePath()
    End Function

    Public Function WriteXml(dir$) As Boolean
        Dim path$ = dir & "/" & filePath()
        Dim xml$ = toXml()
        Return xml.SaveTo(path, TextEncodings.UTF8WithoutBOM)
    End Function
End Class
