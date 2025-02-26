#Region "Microsoft.VisualBasic::22d56631ffeb9c765ed0211ea2cc4182, Data\DataFrame.Extensions\Serialize\ObjectSchema\Field.vb"

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

    '   Total Lines: 57
    '    Code Lines: 33 (57.89%)
    ' Comment Lines: 16 (28.07%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (14.04%)
    '     File Size: 1.93 KB


    '     Class Field
    ' 
    '         Properties: Binding, BindProperty, InnerClass, Name, Type
    ' 
    '         Function: GetValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection

Namespace Serialize.ObjectSchema

    ''' <summary>
    ''' + ``#`` uid;
    ''' + ``[FiledName]`` This field links to a external file, and id is point to the ``#`` uid field in the external file.
    ''' </summary>
    Public Class Field : Implements IReadOnlyId

        ''' <summary>
        ''' Field Name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String Implements IReadOnlyId.Identity
            Get
                Return Binding.Name
            End Get
        End Property

        Public ReadOnly Property BindProperty As PropertyInfo
            Get
                Return Binding.BindProperty
            End Get
        End Property

        Public ReadOnly Property Type As Type
            Get
                Return BindProperty.PropertyType
            End Get
        End Property

        ''' <summary>
        ''' 首先DirectCast为<see cref="IAttributeComponent"/>类型
        ''' </summary>
        ''' <returns></returns>
        Public Property Binding As ComponentModels.StorageProvider
        ''' <summary>
        ''' 假若这个为Nothing，则说明当前的域是简单类型
        ''' </summary>
        ''' <returns></returns>
        Public Property InnerClass As [Class]

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetValue(x As Object) As Object
            Return Binding.BindProperty.GetValue(x, Nothing)
        End Function

        Public Overrides Function ToString() As String
            Return Binding.ToString
        End Function
    End Class
End Namespace
