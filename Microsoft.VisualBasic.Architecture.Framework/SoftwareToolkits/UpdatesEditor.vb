#Region "Microsoft.VisualBasic::ee421ad28c4bf1014cbc1aa024e5324b, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\SoftwareToolkits\UpdatesEditor.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Friend Class UpdatesEditor : Implements ComponentModel.ITextFile.IDocumentEditor

    Private Sub UpdatesEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Public Property DocumentPath As String Implements ComponentModel.ITextFile.IDocumentEditor.DocumentPath

    Public Function LoadDocument(Path As String) As Boolean Implements ComponentModel.ITextFile.IDocumentEditor.LoadDocument
        Throw New NotImplementedException
    End Function

    Public Function Save(Optional FilePath As String = "", Optional Encoding As Text.Encoding = Nothing) As Boolean Implements ComponentModel.ITextFile.IDocumentEditor.Save
        Throw New NotImplementedException
    End Function
End Class
