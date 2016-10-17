#Region "Microsoft.VisualBasic::fdc7c70d99080de75c6807d8aeb3b5db, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\UpdatesEditor.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
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

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text

Namespace SoftwareToolkits

    Friend Class UpdatesEditor : Implements IDocumentEditor

        Private Sub UpdatesEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        End Sub

        Public Property DocumentPath As String Implements IDocumentEditor.DocumentPath

        Public Function LoadDocument(Path As String) As Boolean Implements IDocumentEditor.LoadDocument
            Throw New NotImplementedException
        End Function

        Public Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean Implements ComponentModel.IDocumentEditor.Save
            Throw New NotImplementedException
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.GetEncodings)
        End Function
    End Class
End Namespace