#Region "Microsoft.VisualBasic::3ba2d1025c73c577a3c7edff6d00f895, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\_rels\rels.vb"

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

    '   Total Lines: 42
    '    Code Lines: 29 (69.05%)
    ' Comment Lines: 5 (11.90%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (19.05%)
    '     File Size: 1.23 KB


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

        ''' <summary>
        ''' load xml file
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        Public Shared Function Load(file As String) As rels
            Return New rels With {
                .document = file.SolveStream.LoadFromXml(Of OpenXml.rels)
            }
        End Function

        Public Overrides Function ToString() As String
            Return filePath()
        End Function
    End Class
End Namespace
