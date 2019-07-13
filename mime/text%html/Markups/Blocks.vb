#Region "Microsoft.VisualBasic::c02f78e3cae82904e1b4b31c13df3995, mime\text%html\Markups\Blocks.vb"

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

    ' Class Header
    ' 
    '     Properties: Level
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Header : Inherits PlantText

    Public Property Level As Integer

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
