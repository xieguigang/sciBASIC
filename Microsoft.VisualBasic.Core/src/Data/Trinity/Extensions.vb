#Region "Microsoft.VisualBasic::ebc0acb27e6002bb75c2f03711b0f9ac, Microsoft.VisualBasic.Core\src\Data\Trinity\Extensions.vb"

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

    '   Total Lines: 24
    '    Code Lines: 19 (79.17%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 718 B


    '     Module Extensions
    ' 
    '         Properties: ClassTable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetClass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Trinity.NLP
Imports Microsoft.VisualBasic.Language

Namespace Data.Trinity

    Public Module Extensions

        Public ReadOnly Property ClassTable As IReadOnlyDictionary(Of String, WordClass)

        Sub New()
            ClassTable = Enums(Of WordClass).ToDictionary(Function(c) c.Description)
        End Sub

        Public Function GetClass(tag As String) As WordClass
            With LCase(tag)
                If ClassTable.ContainsKey(.ByRef) Then
                    Return ClassTable(.ByRef)
                Else
                    Return WordClass.NA
                End If
            End With
        End Function
    End Module
End Namespace
