#Region "Microsoft.VisualBasic::3be836190f27e2289be3669c639d36b4, Data\BinaryData\BinaryData\XDR\MapException.vb"

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

    '     Class MapException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ReadFix, ReadOne, ReadVar, WriteFix, WriteOne
    '                   WriteVar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    Public Class MapException
        Inherits SystemException

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(message As String, innerEx As Exception)
            MyBase.New(message, innerEx)
        End Sub

        Public Shared Function ReadOne(type As Type, innerEx As Exception) As MapException
            Return New MapException(String.Format("can't read an instance of `{0}'", type.FullName), innerEx)
        End Function

        Friend Shared Function ReadVar(type As Type, max As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't read collection of `{0}' (length <= {1})", type.FullName, max), innerEx)
        End Function

        Friend Shared Function ReadFix(type As Type, len As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't read collection of `{0}' (length = {1})", type.FullName, len), innerEx)
        End Function

        Friend Shared Function WriteOne(type As Type, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write an instance of `{0}'", type.FullName), innerEx)
        End Function

        Friend Shared Function WriteFix(type As Type, len As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write collection of `{0}' (length = {1})", type.FullName, len), innerEx)
        End Function

        Friend Shared Function WriteVar(type As Type, max As UInteger, innerEx As SystemException) As Exception
            Return New MapException(String.Format("can't write collection of `{0}' (length <= {1})", type.FullName, max), innerEx)
        End Function
    End Class
End Namespace

