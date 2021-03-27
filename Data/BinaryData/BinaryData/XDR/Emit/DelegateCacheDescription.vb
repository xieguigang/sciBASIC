#Region "Microsoft.VisualBasic::d9c9f765057d5f82049889558353172d, Data\BinaryData\BinaryData\XDR\Emit\DelegateCacheDescription.vb"

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

    '     Class BuildBinderDescription
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Reflection.Emit
Imports System.Reflection

Namespace Xdr.Emit
    Public Class BuildBinderDescription
        Public ReadOnly Result As Type
        Public ReadOnly BuildRequest As FieldInfo

        Public Sub New(modBuilder As ModuleBuilder)
            Dim typeBuilder = modBuilder.DefineType("BuildBinder", TypeAttributes.Public Or TypeAttributes.Class Or TypeAttributes.Abstract Or TypeAttributes.Sealed)
            Dim fb_request = typeBuilder.DefineField("Request", GetType(Action(Of Type, OpaqueType)), FieldAttributes.Public Or FieldAttributes.Static)
            Result = typeBuilder.CreateType()
            BuildRequest = fb_request
        End Sub
    End Class
End Namespace

