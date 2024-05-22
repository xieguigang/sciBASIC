#Region "Microsoft.VisualBasic::2fe735b6df65338a8fb2f6d4779b8714, Data\BinaryData\HDF5\dataset\filters\filters.vb"

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

    '   Total Lines: 30
    '    Code Lines: 20 (66.67%)
    ' Comment Lines: 3 (10.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (23.33%)
    '     File Size: 1.12 KB


    '     Class DeflatePipelineFilter
    ' 
    '         Properties: id, name
    ' 
    '         Function: decode
    ' 
    '     Class Fletcher32CheckSum
    ' 
    '         Properties: id, name
    ' 
    '         Function: decode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.SecurityString

Namespace dataset.filters

    ''' <summary>
    ''' GZip
    ''' </summary>
    Public Class DeflatePipelineFilter : Implements IFilter

        Public ReadOnly Property id As Integer Implements IFilter.id
        Public ReadOnly Property name As String Implements IFilter.name

        Public Function decode(encodedData() As Byte, filterData() As Integer) As Byte() Implements IFilter.decode
            Return encodedData.UnZipStream(noMagic:=True).ToArray
        End Function
    End Class

    Public Class Fletcher32CheckSum : Implements IFilter

        Public ReadOnly Property id As Integer Implements IFilter.id
        Public ReadOnly Property name As String Implements IFilter.name

        Public Function decode(encodedData() As Byte, filterData() As Integer) As Byte() Implements IFilter.decode
            Dim checksum = encodedData.Fletcher32(Scan0, encodedData.Length)
            Return BitConverter.GetBytes(checksum)
        End Function
    End Class
End Namespace
