#Region "Microsoft.VisualBasic::e5fa6a06bdb08f22d64257f415082bd9, Microsoft.VisualBasic.Core\src\Extensions\IO\IOStream.vb"

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
    '    Code Lines: 41 (77.36%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (22.64%)
    '     File Size: 1.62 KB


    '     Class IOStream
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Operators: >>
    ' 
    '     Module IOStreamExtensions
    ' 
    '         Function: AsIOStream, FileOpen
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.My.FrameworkInternal.IOHandler
Imports Microsoft.VisualBasic.Text

Namespace FileIO

    Public Class IOStream(Of T)

        Shared ReadOnly save As ISave
        Shared ReadOnly obj As Type = GetType(T)

        Dim data As Object

        Shared Sub New()
            If IOHandler.IsRegister(obj) Then
                save = IOHandler.GetWrite(obj)
            Else
                save = IOHandler.GetWrite(GetType(IEnumerable(Of T)))
            End If
        End Sub

        Sub New(data As IEnumerable(Of T))
            Me.data = data
        End Sub

        Sub New(data As T)
            Me.data = data
        End Sub

        Public Shared Operator >>(stream As IOStream(Of T), handle%) As Boolean
            Return IOStream(Of T).save(stream.data, My.File.GetHandle(handle).FileName, DefaultEncoding)
        End Operator

    End Class

    <HideModuleName>
    <Extension>
    Public Module IOStreamExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsIOStream(Of T)(data As IEnumerable(Of T)) As IOStream(Of T)
            Return New IOStream(Of T)(data)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FileOpen(path As String, Optional encoding As Encodings = Encodings.UTF8) As Integer
            Return My.File.OpenHandle(path, encoding)
        End Function
    End Module
End Namespace
