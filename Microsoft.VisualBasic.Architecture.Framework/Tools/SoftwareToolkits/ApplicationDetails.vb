Imports System.Collections.Generic
Imports System.Text
Imports System.Reflection
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SoftwareToolkits

    ''' <summary>
    ''' Parsing product assembly meta data
    ''' </summary>
    ''' <remarks>
    ''' http://www.c-sharpcorner.com/uploadfile/ravesoft/access-assemblyinfo-file-and-get-product-informations/
    ''' </remarks>
    Public Class ApplicationDetails

        Public ReadOnly Property Assembly As Assembly

        Sub New(assm As Assembly)
            Assembly = assm
            CompanyName = GetCompanyName(assm)
            ProductVersion = GetProductVersion(assm)
            ProductName = GetProductName(assm)
            CopyRightsDetail = GetCopyRightsDetail(assm)
            ProductTitle = GetProductTitle(assm)
            ProductDescription = GetProductDescription(assm)
        End Sub

        ''' <summary>
        ''' 获取当前的应用程序的描述信息
        ''' </summary>
        Sub New()
            Call Me.New(GetType(ApplicationDetails).Assembly)
        End Sub

        Public Shared Function GetCompanyName(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim companyName As String = ""
                Dim customAttributes As Object() = assm.GetCustomAttributes(GetType(AssemblyCompanyAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    companyName = DirectCast(customAttributes(0), AssemblyCompanyAttribute).Company
                End If
                If String.IsNullOrEmpty(companyName) Then
                    companyName = String.Empty
                End If

                Return companyName
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the name of the system provider name from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CompanyName() As String

        Public Shared Function GetProductVersion(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productVersion As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyVersionAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productVersion = DirectCast(customAttributes(0), AssemblyVersionAttribute).Version
                End If
                If String.IsNullOrEmpty(productVersion) Then
                    productVersion = String.Empty
                End If
                Return productVersion
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get System version from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductVersion() As String

        Public Shared Function GetProductName(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productName As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyProductAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productName = DirectCast(customAttributes(0), AssemblyProductAttribute).Product
                End If
                If String.IsNullOrEmpty(productName) Then
                    productName = String.Empty
                End If
                Return productName
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the name of the System from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductName() As String

        Public Shared Function GetCopyRightsDetail(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim copyrightsDetail As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyCopyrightAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    copyrightsDetail = DirectCast(customAttributes(0), AssemblyCopyrightAttribute).Copyright
                End If
                If String.IsNullOrEmpty(copyrightsDetail) Then
                    copyrightsDetail = String.Empty
                End If
                Return copyrightsDetail
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the copyRights details from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CopyRightsDetail() As String

        Public Shared Function GetProductTitle(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productTitle As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productTitle = DirectCast(customAttributes(0), AssemblyTitleAttribute).Title
                End If
                If String.IsNullOrEmpty(productTitle) Then
                    productTitle = String.Empty
                End If
                Return productTitle
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the Product tile from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductTitle() As String

        Public Shared Function GetProductDescription(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productDescription As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyDescriptionAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productDescription = DirectCast(customAttributes(0), AssemblyDescriptionAttribute).Description
                End If
                If String.IsNullOrEmpty(productDescription) Then
                    productDescription = String.Empty
                End If
                Return productDescription
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the description of the product from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductDescription() As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace


