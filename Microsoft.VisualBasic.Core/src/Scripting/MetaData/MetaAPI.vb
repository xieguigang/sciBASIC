#Region "Microsoft.VisualBasic::a4ac39b554ee308d53f58010efa121b0, Microsoft.VisualBasic.Core\src\Scripting\MetaData\MetaAPI.vb"

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

    '   Total Lines: 55
    '    Code Lines: 40 (72.73%)
    ' Comment Lines: 9 (16.36%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (10.91%)
    '     File Size: 2.02 KB


    '     Module MetaAPI
    ' 
    '         Properties: TypeInfo
    ' 
    '         Function: (+2 Overloads) GetCLIMod, GetEntry
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices

Namespace Scripting.MetaData

    Public Module MetaAPI

        ''' <summary>
        ''' <see cref="PackageAttribute"/>
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TypeInfo As Type = GetType(PackageAttribute)

        <Extension> Public Function GetCLIMod(assm As Assembly) As Type
            Dim types As Type() = assm.GetTypes
            Dim LQuery = (From type As Type In types
                          Let attrs As Object() = type.GetCustomAttributes(PackageAttribute.TypeInfo, inherit:=True)
                          Where Not attrs.IsNullOrEmpty
                          Let attr = DirectCast(attrs(Scan0), PackageAttribute)
                          Select attr, type)
            Dim GetCLI = (From x In LQuery
                          Where x.attr.Category = APICategories.CLI_MAN
                          Select x.type).FirstOrDefault
            Return GetCLI
        End Function

        Public Function GetCLIMod(path As String) As Type
            Try
                Dim assm As Assembly = Assembly.LoadFile(path)
                Return assm.GetCLIMod
            Catch ex As Exception
                ex = New Exception(path.ToFileURL, ex)
                Call App.LogException(ex)
#If DEBUG Then
                Call ex.PrintException
#End If
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' 获取定义在类型定义上面的命名空间的标记信息
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function GetEntry(type As Type) As PackageAttribute
#If NET_40 = 0 Then
            Dim attrs = type.GetCustomAttributes(Of PackageAttribute)(inherit:=True)
            Return attrs.FirstOrDefault
#Else
            Throw New NotSupportedException
#End If
        End Function
    End Module
End Namespace
