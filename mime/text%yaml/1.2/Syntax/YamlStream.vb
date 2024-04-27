﻿#Region "Microsoft.VisualBasic::508c95cdd6edcde824e25c1e276872c0, G:/GCModeller/src/runtime/sciBASIC#/mime/text%yaml//1.2/Syntax/YamlStream.vb"

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

    '   Total Lines: 20
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 638 B


    '     Class YamlStream
    ' 
    '         Properties: Documents
    ' 
    '         Function: __maps, Enumerative
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Syntax

    Public Class YamlStream

        Public Property Documents As New List(Of YamlDocument)()

        Public Iterator Function Enumerative() As IEnumerable(Of Dictionary(Of MappingEntry))
            For Each doc As YamlDocument In Documents
                Yield __maps(doc)
            Next
        End Function

        Private Function __maps(doc As YamlDocument) As Dictionary(Of MappingEntry)
            Dim root As Mapping = DirectCast(doc.Root, Mapping)
            Return root.GetMaps
        End Function
    End Class
End Namespace
