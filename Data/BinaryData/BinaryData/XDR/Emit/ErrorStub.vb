#Region "Microsoft.VisualBasic::e6da63e5707615687bc169b000d8cd3d, Data\BinaryData\BinaryData\XDR\Emit\ErrorStub.vb"

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

    '     Module ErrorStub
    ' 
    '         Function: ReadManyDelegate, ReadOneDelegate, StubDelegate, WriteManyDelegate, WriteOneDelegate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace Xdr
    Friend Module ErrorStub
        Private Function StubDelegate(ex As Exception, method As String, targetType As Type, genDelegateType As Type) As [Delegate]
            Dim stubType = GetType(ErrorStubType(Of)).MakeGenericType(targetType)
            Dim stubInstance = Activator.CreateInstance(stubType, ex)
            Dim mi = stubType.GetMethod(method)
            Return [Delegate].CreateDelegate(genDelegateType.MakeGenericType(targetType), stubInstance, mi)
        End Function

        Friend Function ReadOneDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "ReadOne", t, GetType(ReadOneDelegate(Of)))
        End Function

        Friend Function ReadManyDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "ReadMany", t, GetType(ReadManyDelegate(Of)))
        End Function

        Friend Function WriteOneDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "WriteOne", t, GetType(WriteOneDelegate(Of)))
        End Function

        Friend Function WriteManyDelegate(t As Type, ex As Exception) As [Delegate]
            Return StubDelegate(ex, "WriteMany", t, GetType(WriteManyDelegate(Of)))
        End Function
    End Module
End Namespace

