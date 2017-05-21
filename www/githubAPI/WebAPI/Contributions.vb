Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace WebAPI

    Public Module Contributions

        ''' <summary>
        ''' https://github.com/users/xieguigang/contributions
        ''' </summary>
        ''' <param name="userName$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetUserContributions(userName$) As Dictionary(Of Date, Integer)
            Dim url$ = $"https://github.com/users/{userName}/contributions"
            Dim svg$ = url.GET
            Dim xml As New XmlDocument
            Call xml.LoadXml("<?xml version=""1.0"" encoding=""utf-8""?>" & vbCrLf & svg)
            Dim g As XmlNodeList = xml _
                .SelectSingleNode("svg") _
                .SelectSingleNode("g") _
                .SelectNodes("g")
            Dim contributions As New Dictionary(Of Date, Integer)

            For Each week As XmlNode In g
                Dim days = week.SelectNodes("rect")

                For Each day As XmlNode In days
                    Dim date$ = day.Attributes.GetNamedItem("data-date").InnerText
                    Dim count = day.Attributes.GetNamedItem("data-count").InnerText
                    contributions(DateTime.Parse([date])) = CInt(count)
                Next
            Next

            Return contributions
        End Function
    End Module
End Namespace