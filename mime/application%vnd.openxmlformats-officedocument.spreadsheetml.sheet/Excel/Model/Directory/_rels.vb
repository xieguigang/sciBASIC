#Region "Microsoft.VisualBasic::48430c7195ffb4cb159cc525f73e1c86, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\Directory\_rels.vb"

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

    '     Class _rels
    ' 
    '         Properties: rels, workbook
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _name
    ' 
    '         Sub: _loadContents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels

Namespace Model.Directory

    Public Class _rels : Inherits Directory

        ''' <summary>
        ''' ``.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property rels As rels
        ''' <summary>
        ''' ``workbook.xml.rels``
        ''' </summary>
        ''' <returns></returns>
        Public Property workbook As rels

        Sub New(ROOT$)
            Call MyBase.New(ROOT)
        End Sub

        Protected Overrides Sub _loadContents()
            Dim path As Value(Of String) = ""

            If (path = Folder & "/.rels").FileExists Then
                rels = (+path).LoadXml(Of rels)
            End If
            If (path = Folder & "/workbook.xml.rels").FileExists Then
                workbook = (+path).LoadXml(Of rels)
            End If
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(_rels)
        End Function
    End Class
End Namespace
