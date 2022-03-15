#Region "Microsoft.VisualBasic::86239ace0ce06bc036699a75623d7f06, sciBASIC#\Data\DataFrame\IO\NetStream\StreamHelper.vb"

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

    '   Total Lines: 78
    '    Code Lines: 0
    ' Comment Lines: 62
    '   Blank Lines: 16
    '     File Size: 2.50 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::358e468d33d7ef53c45d5e57acf2986b, Data\DataFrame\IO\NetStream\StreamHelper.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xie (genetics@smrucc.org)
'    '       xieguigang (xie.guigang@live.com)
'    ' 
'    ' Copyright (c) 2018 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



'    ' /********************************************************************************/

'    ' Summaries:

'    '     Module StreamHelper
'    ' 
'    '         Function: GetBytes, LoadHelper
'    '         Class __load
'    ' 
'    '             Constructor: (+1 Overloads) Sub New
'    '             Function: Load
'    ' 
'    ' 
'    ' 
'    ' 
'    ' /********************************************************************************/

'#End Region

'Imports Microsoft.VisualBasic.Serialization.BinaryDumping
'Imports Microsoft.VisualBasic.Text

'Namespace IO.NetStream

'    Module StreamHelper

'        Public Function GetBytes(x As RowObject) As Byte()
'            Return x.Serialize
'        End Function

'        Public Function LoadHelper(encoding As Encodings) As IGetObject(Of RowObject)
'            Dim helper As New EncodingHelper(encoding)
'            Return AddressOf New __load(helper).Load
'        End Function

'        Private Class __load

'            ReadOnly __encoding As EncodingHelper

'            Sub New(encoding As EncodingHelper)
'                __encoding = encoding
'            End Sub

'            Public Function Load(byts As Byte()) As RowObject
'                Return New RowObject(byts, __encoding)
'            End Function
'        End Class
'    End Module
'End Namespace
