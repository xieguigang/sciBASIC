#Region "Microsoft.VisualBasic::95fb1dad6e52689ae619566b466a7ad2, www\Microsoft.VisualBasic.NETProtocol\NETProtocol\Protocols\POST.vb"

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
    '    Code Lines: 14
    ' Comment Lines: 4
    '   Blank Lines: 6
    '     File Size: 586 B


    '     Class InitPOSTBack
    ' 
    '         Properties: Portal, uid
    ' 
    '     Class UserId
    ' 
    '         Properties: sId, uid
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NETProtocol.Protocols

    Public Class InitPOSTBack
        ''' <summary>
        ''' 长连接socket的端口终点
        ''' </summary>
        ''' <returns></returns>
        Public Property Portal As IPEndPoint
        Public Property uid As Long
    End Class

    Public Class UserId
        Public Property uid As Long
        Public Property sId As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class


End Namespace
