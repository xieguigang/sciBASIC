#Region "Microsoft.VisualBasic::ed14054e733e8383d7695ad915c60726, gr\network-visualization\Network.IO.Extensions\IO\FileStream\MetaData.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13 (72.22%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (27.78%)
    '     File Size: 529 B


    '     Class MetaData
    ' 
    '         Properties: additionals, create_time, creators, description, keywords
    '                     links, title
    ' 
    '         Function: Union
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace FileStream

    Public Class MetaData

        Public Property creators As String()
        Public Property description As String
        Public Property title As String
        Public Property create_time As String
        Public Property links As String()
        Public Property keywords As String()

        Public Property additionals As Dictionary(Of String, String)

        Public Shared Function Union(a As MetaData, b As MetaData) As MetaData
            Throw New NotImplementedException
        End Function
    End Class
End Namespace
