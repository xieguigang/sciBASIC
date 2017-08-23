#Region "Microsoft.VisualBasic::c3ee928041f747c99d939798e0977ea9, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\CLIMapper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace CommandLine

    ''' <summary>
    ''' 从可写属性之中赋值
    ''' </summary>
    Public Module CLIMapper

        ''' <summary>
        ''' Assign the argument value in the commandline into the target argument container object.
        ''' The properties in the container class type needs decorating with attribute 
        ''' <see cref="CLIParameter"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="args"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Maps(Of T As Class)(args As CommandLine, Optional strict As Boolean = False) As T
            Dim type As Type = GetType(T)
            Dim obj As Object = Activator.CreateInstance(type)
            Dim props = From prop As PropertyInfo
                        In type.GetProperties
                        Where prop.CanWrite AndAlso
                            Scripting.IsPrimitive(prop.PropertyType)
                        Select prop

            For Each prop As PropertyInfo In props
                Dim name As String = prop.GetName

                If Not args.ContainsParameter(name, Not strict) Then
                    Continue For
                Else
                    type = prop.PropertyType
                End If

                If type.Equals(GetType(Boolean)) Then
                    ' 由于是逻辑值，所以只要存在就是真，不存在就是False
                    Call prop.SetValue(obj, True, Nothing)
                Else
                    Dim s As String = args(name)
                    Dim value = Scripting.CTypeDynamic(s, type)

                    Call prop.SetValue(obj, value, Nothing)
                End If
            Next

            Return DirectCast(obj, T)
        End Function

        ''' <summary>
        ''' If the property <paramref name="prop"/> have the custom attribute <see cref="CLIParameter"/>
        ''' then the name value in <see cref="CLIParameter"/> will be used, otherwise, 
        ''' <see cref="PropertyInfo.Name"/> will be used. 
        ''' </summary>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        <Extension> Public Function GetName(prop As PropertyInfo) As String
            Dim attr As CLIParameter = prop.GetAttribute(Of CLIParameter)

            If attr Is Nothing OrElse attr.Name.StringEmpty Then
                Return prop.Name
            Else
                Return attr.Name
            End If
        End Function
    End Module
End Namespace
