#Region "Microsoft.VisualBasic::bd6bc82c9139c85f1335c39ceaab4828, gr\Microsoft.VisualBasic.Imaging\Drivers\Models\PSData.vb"

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

    '   Total Lines: 28
    '    Code Lines: 22 (78.57%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 809 B


    '     Class PostScriptData
    ' 
    '         Properties: Driver
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDataURI, Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Net.Http

Namespace Driver

    Public Class PostScriptData : Inherits GraphicsData

        Public Overrides ReadOnly Property Driver As Drivers
            Get
                Return Drivers.PostScript
            End Get
        End Property

        Public Sub New(img As Object, size As Size, padding As Padding)
            MyBase.New(img, size, padding)
        End Sub

        Public Overrides Function GetDataURI() As DataURI
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Save(out As Stream) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
