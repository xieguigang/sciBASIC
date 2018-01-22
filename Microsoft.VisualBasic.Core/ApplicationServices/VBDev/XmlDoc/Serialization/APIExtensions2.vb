Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Language

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Partial Module APIExtensions

        <Extension> Public Function Sum(projects As IEnumerable(Of Project), <CallerMemberName> Optional name$ = Nothing) As Project
            Dim namespaces As New Dictionary(Of String, ProjectNamespace)
            Dim out As New Project(name, namespaces)

            For Each proj As Project In projects
                For Each ns In proj.Namespaces
                    With ns.Path.ToLower
                        If namespaces.ContainsKey(.ref) Then
                            namespaces(.ref) = namespaces(.ref).Add(ns, proj:=out)
                        Else
                            namespaces.Add(.ref, ns)
                        End If
                    End With
                Next
            Next

            Return out
        End Function

        <Extension>
        Private Function Add(ns1 As ProjectNamespace, ns2 As ProjectNamespace, proj As Project) As ProjectNamespace
            Dim path$ = ns1.Path
            Dim types As New Dictionary(Of String, ProjectType)

            For Each type As ProjectType In ns1.Types + ns2.Types.AsList
                Dim key = type.Name.ToLower

                If types.ContainsKey(key) Then

                Else
                    types.Add(key, type)
                End If
            Next

            Return New ProjectNamespace(proj, types) With {
                .Path = path
            }
        End Function
    End Module
End Namespace