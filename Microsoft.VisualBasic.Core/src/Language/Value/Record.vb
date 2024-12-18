#Region "Microsoft.VisualBasic::865dd247b916e144a04637346de03322, Microsoft.VisualBasic.Core\src\Language\Value\Record.vb"

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

    '   Total Lines: 70
    '    Code Lines: 45 (64.29%)
    ' Comment Lines: 13 (18.57%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 12 (17.14%)
    '     File Size: 2.74 KB


    '     Class Record
    ' 
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Language

    ''' <summary>
    ''' Data record type in VisualBasic
    ''' </summary>
    Public MustInherit Class Record

        ''' <summary>
        ''' Make check of the value equals?
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(a As Record, b As Record) As Boolean
            If a Is Nothing AndAlso b Is Nothing Then
                Return True
            ElseIf a Is Nothing OrElse b Is Nothing Then
                Return False
            End If

            Static schema As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            Dim t1 = schema.ComputeIfAbsent(a.GetType, lazyValue:=Function(t) DataFramework.Schema(t, PropertyAccess.Readable, nonIndex:=True))
            Dim t2 = schema.ComputeIfAbsent(b.GetType, lazyValue:=Function(t) DataFramework.Schema(t, PropertyAccess.Readable, nonIndex:=True))

            ' is not identical type, andalso property name is not equals
            ' assert that value is not equals directly
            If a.GetType IsNot b.GetType AndAlso t1.Keys.Union(t2.Keys).Count <> t1.Count Then
                Return False
            End If

            ' has the same identical keys
            For Each name As String In t1.Keys
                If t1(name).PropertyType IsNot t2(name).PropertyType Then
                    If t1(name).PropertyType.IsInheritsFrom(GetType(Record)) AndAlso
                        t2(name).PropertyType.IsInheritsFrom(GetType(Record)) Then

                        If DirectCast(t1(name).GetValue(a), Record) <> DirectCast(t2(name).GetValue(b), Record) Then
                            Return False
                        End If
                    Else
                        Return False
                    End If
                Else
                    Dim val1 As Object = t1(name).GetValue(a)
                    Dim val2 As Object = t2(name).GetValue(b)

                    If Not Object.Equals(val1, val2) Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(a As Record, b As Record) As Boolean
            Return Not a = b
        End Operator

    End Class
End Namespace
