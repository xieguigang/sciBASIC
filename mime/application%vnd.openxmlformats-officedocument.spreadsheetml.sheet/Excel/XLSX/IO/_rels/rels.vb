#Region "Microsoft.VisualBasic::288023653308bf4863949a804dbb6a73, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\_rels\rels.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 37
    '    Code Lines: 29
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.06 KB


    '     Class rels
    ' 
    '         Properties: document, Target
    ' 
    '         Function: filePath, Load, ToString, toXml
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.OpenXml

Namespace XLSX.XML._rels

    Public Class rels : Implements IXml

        Public Property document As OpenXml.rels

        Public Property Target(Id As String) As Relationship
            Get
                Return _document(Id)
            End Get
            Set(value As Relationship)
                _document(Id) = value
            End Set
        End Property

        Private Function filePath() As String Implements IXml.filePath
            Return "_rels/.rels"
        End Function

        Private Function toXml() As String Implements IXml.toXml
            Return document.GetXml
        End Function

        Public Shared Function Load(file As String) As rels
            Return New rels With {
                .document = file.LoadXml(Of OpenXml.rels)
            }
        End Function

        Public Overrides Function ToString() As String
            Return filePath()
        End Function
    End Class
End Namespace
