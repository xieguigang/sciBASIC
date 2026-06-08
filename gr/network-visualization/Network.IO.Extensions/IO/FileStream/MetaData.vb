#Region "Microsoft.VisualBasic::36acd1b8a7225c782da3eb86f6a95d30, gr\network-visualization\Network.IO.Extensions\IO\FileStream\MetaData.vb"

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

    '   Total Lines: 21
    '    Code Lines: 14 (66.67%)
    ' Comment Lines: 3 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (19.05%)
    '     File Size: 643 B


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

    ''' <summary>
    ''' the network metadata
    ''' </summary>
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
