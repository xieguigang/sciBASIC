#Region "Microsoft.VisualBasic::3783fe2a91209660cae73e35e945cfb3, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Extensions\APIExtensions2.vb"

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

    '   Total Lines: 93
    '    Code Lines: 77 (82.80%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (17.20%)
    '     File Size: 3.76 KB


    '     Module APIExtensions
    ' 
    '         Function: (+2 Overloads) Add, mergeAnnotations, (+2 Overloads) Sum
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Partial Module APIExtensions

        <Extension> Public Function Sum(projects As IEnumerable(Of Project), Optional excludeNamespace$() = Nothing, <CallerMemberName> Optional name$ = Nothing) As Project
            Dim namespaces As New Dictionary(Of String, ProjectNamespace)
            Dim out As New Project(name, namespaces)

            For Each proj As Project In projects
                For Each ns In proj.Namespaces

                    If Not excludeNamespace Is Nothing Then
                        Dim skip As Boolean = False

                        For Each nsName As String In excludeNamespace
                            If InStr(ns.fullName, nsName) = 1 Then
                                skip = True
                                Exit For
                            End If
                        Next

                        If skip Then
                            Continue For
                        End If
                    End If

                    With ns.fullName.ToLower
                        If namespaces.ContainsKey(.ByRef) Then
                            namespaces(.ByRef) = namespaces(.ByRef).Add(ns, proj:=out)
                        Else
                            namespaces.Add(.ByRef, ns)
                        End If
                    End With
                Next
            Next

            Return out
        End Function

        <Extension>
        Public Function Sum(members As IEnumerable(Of ProjectMember), type As ProjectType) As ProjectMember
            Dim list = members.ToArray
            Return New ProjectMember(type) With {
                .Name = list(0).Name,
                .Declare = list.Select(Function(m) m.Declare).Distinct.JoinBy(vbLf),
                .Remarks = list.Select(Function(m) m.Remarks).Distinct.JoinBy(vbLf),
                .Returns = list.Select(Function(m) m.Returns).Distinct.JoinBy(vbLf),
                .Summary = list.Select(Function(m) m.Summary).Distinct.JoinBy(vbLf),
                .Params = list.Select(Function(m) m.Params).IteratesALL.ToArray
            }
        End Function

        <Extension>
        Private Function Add(ns1 As ProjectNamespace, ns2 As ProjectNamespace, proj As Project) As ProjectNamespace
            Dim path$ = ns1.fullName
            Dim types As New Dictionary(Of String, ProjectType)

            For Each type As ProjectType In ns1.Types + ns2.Types.AsList
                Dim key = type.Name.ToLower

                If types.ContainsKey(key) Then
                    types(key) = types(key).Add(type)
                Else
                    types.Add(key, type)
                End If
            Next

            Return New ProjectNamespace(proj, types) With {
                .fullName = path
            }
        End Function

        <Extension>
        Private Function Add(t1 As ProjectType, t2 As ProjectType) As ProjectType
            Return New ProjectType(t1, t2) With {
                .Name = t1.Name,
                .Remarks = mergeAnnotations(t1.Remarks, t2.Remarks),
                .Summary = mergeAnnotations(t1.Summary, t2.Summary)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Function mergeAnnotations(a$, b$) As String
            Return {a, b}.Distinct.JoinBy(ASCII.LF & ASCII.LF)
        End Function
    End Module
End Namespace
