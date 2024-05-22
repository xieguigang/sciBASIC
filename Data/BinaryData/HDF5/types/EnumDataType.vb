#Region "Microsoft.VisualBasic::a3cb10a192ce0db76c45d36cabd4a1e1, Data\BinaryData\HDF5\types\EnumDataType.vb"

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

    '   Total Lines: 31
    '    Code Lines: 11 (35.48%)
    ' Comment Lines: 14 (45.16%)
    '    - Xml Docs: 28.57%
    ' 
    '   Blank Lines: 6 (19.35%)
    '     File Size: 708 B


    '     Class EnumDataType
    ' 
    '         Properties: BaseType, EnumMapping, TypeInfo
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  This file is part of jHDF. A pure Java library for accessing HDF5 files.
' 
'  http://jhdf.io
' 
'  Copyright (c) 2022 James Mudd
' 
'  MIT License see 'LICENSE' file
' 
Namespace type


    ''' <summary>
    ''' Class for reading enum data type messages.
    ''' 
    ''' @author James Mudd
    ''' </summary>
    Public Class EnumDataType : Inherits DataType

        Public Property BaseType As DataType

        Public Property EnumMapping As IDictionary(Of Integer?, String)

        Public Overrides ReadOnly Property TypeInfo As System.Type
            Get
                Return GetType(String)
            End Get
        End Property
    End Class

End Namespace
