#Region "Microsoft.VisualBasic::0b191b9ac322c3bf2a7b7b816ba148a0, Microsoft.VisualBasic.Core\Serialization\BinaryDumping\BinaryWriter.vb"

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

    '     Module BinaryWriter
    ' 
    '         Function: __serialize, GetReadProperty, (+2 Overloads) Serialization
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Serialization.BinaryDumping

    Public Module BinaryWriter

        <Extension> Public Function Serialization(Of T)(obj As T) As Byte()
            Dim type As Type = GetType(T)
            Return Serialization(obj, type).ToArray
        End Function

        Public Function Serialization(obj As Object, type As Type) As List(Of Byte)
            Dim visited As New List(Of Object)
            Dim readProps As PropertyInfo() = type.GetReadProperty
            Dim buffer As New List(Of Byte)

            For Each prop As PropertyInfo In readProps

            Next

            Return buffer
        End Function

        <Extension>
        Public Function GetReadProperty(type As Type) As PropertyInfo()
            Dim LQuery = (From p As PropertyInfo
                          In type.GetProperties(BindingFlags.Public Or BindingFlags.Instance)
                          Where p.CanRead AndAlso
                              p.GetIndexParameters.IsNullOrEmpty
                          Select p).ToArray
            Return LQuery
        End Function

        Private Function __serialize(obj As Object, type As Type, ByRef visited As List(Of Object)) As List(Of Byte)
            Dim readProps As PropertyInfo() = type.GetReadProperty
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
