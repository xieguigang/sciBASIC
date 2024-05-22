#Region "Microsoft.VisualBasic::a77279fefc0374bc033de2e71d8009f8, Data\BinaryData\Feather\Impl\Flatbuffers\IFlatbufferObject.vb"

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

    '   Total Lines: 26
    '    Code Lines: 6 (23.08%)
    ' Comment Lines: 17 (65.38%)
    '    - Xml Docs: 17.65%
    ' 
    '   Blank Lines: 3 (11.54%)
    '     File Size: 927 B


    '     Interface IFlatbufferObject
    ' 
    '         Properties: ByteBuffer
    ' 
    '         Sub: __init
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'* Copyright 2014 Google Inc. All rights reserved.
'*
'* Licensed under the Apache License, Version 2.0 (the "License");
'* you may not use this file except in compliance with the License.
'* You may obtain a copy of the License at
'*
'*     http://www.apache.org/licenses/LICENSE-2.0
'*
'* Unless required by applicable law or agreed to in writing, software
'* distributed under the License is distributed on an "AS IS" BASIS,
'* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'* See the License for the specific language governing permissions and
'* limitations under the License.


Namespace FlatBuffers
    ''' <summary>
    ''' This is the base for both structs and tables.
    ''' </summary>
    Friend Interface IFlatbufferObject
        Sub __init(_i As Integer, _bb As ByteBuffer)

        ReadOnly Property ByteBuffer As ByteBuffer
    End Interface
End Namespace
