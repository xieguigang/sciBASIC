#Region "Microsoft.VisualBasic::b8b60e7f321d9cf7c0eaee12a70edc0c, Data\BinaryData\Feather\Impl\Flatbuffers\Offset.vb"

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

    '   Total Lines: 53
    '    Code Lines: 29
    ' Comment Lines: 17
    '   Blank Lines: 7
    '     File Size: 1.77 KB


    '     Structure Offset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Structure StringOffset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Structure VectorOffset
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
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
    ''' Offset class for typesafe assignments.
    ''' </summary>
    Friend Structure Offset(Of T)
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"Offset(Of {GetType(T).Name}): {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure

    Friend Structure StringOffset
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"StringOffset: {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure

    Friend Structure VectorOffset
        Public Value As Integer
        Public Sub New(value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"VectorOffset: {StringFormats.Lanudry(bytes:=Value)}"
        End Function
    End Structure
End Namespace

