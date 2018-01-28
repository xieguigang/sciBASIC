Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Public Module NamespaceDocExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsNamespaceDoc(type As ProjectType) As Boolean
            Return type.Name = APIExtensions.NamespaceDoc
        End Function

        ''' <summary>
        ''' 一般而言，一个命名空间之下只需要有一个注释类型即可，如果存在多个注释类型的话
        ''' 注释信息字符串会被合并在一起
        ''' </summary>
        ''' <param name="project"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ScanAnnotations(project As ProjectSpace) As Dictionary(Of String, String)
            Dim annotations = project _
                .Select(Function(proj) proj.Namespaces) _
                .IteratesALL _
                .Select(Function(ns) ns.Types) _
                .IteratesALL _
                .Where(AddressOf IsNamespaceDoc) _
                .GroupBy(Function(doc) doc.Namespace.Path) _
                .ToDictionary(Function(ns) ns.Key,
                              Function(nsGroup)
                                  Return nsGroup _
                                      .Select(Function(doc) doc.Summary) _
                                      .Where(Function(s) Not s.StringEmpty) _
                                      .Distinct _
                                      .JoinBy(ASCII.LF & ASCII.LF)
                              End Function)
            Return annotations
        End Function
    End Module
End Namespace