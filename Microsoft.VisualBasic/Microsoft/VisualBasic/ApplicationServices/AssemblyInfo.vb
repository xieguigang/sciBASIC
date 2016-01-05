Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Reflection
Imports System.Security.Permissions

Namespace Microsoft.VisualBasic.ApplicationServices
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class AssemblyInfo
        ' Methods
        Public Sub New(currentAssembly As Assembly)
            If (currentAssembly Is Nothing) Then
                Throw ExceptionUtils.GetArgumentNullException("CurrentAssembly")
            End If
            Me.m_Assembly = currentAssembly
        End Sub

        Private Function GetAttribute(AttributeType As Type) As Object
            Dim customAttributes As Object() = Me.m_Assembly.GetCustomAttributes(AttributeType, True)
            If (customAttributes.Length = 0) Then
                Return Nothing
            End If
            Return customAttributes(0)
        End Function


        ' Properties
        Public ReadOnly Property AssemblyName As String
            Get
                Return Me.m_Assembly.GetName.Name
            End Get
        End Property

        Public ReadOnly Property CompanyName As String
            Get
                If (Me.m_CompanyName Is Nothing) Then
                    Dim attribute As AssemblyCompanyAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyCompanyAttribute)), AssemblyCompanyAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_CompanyName = ""
                    Else
                        Me.m_CompanyName = attribute.Company
                    End If
                End If
                Return Me.m_CompanyName
            End Get
        End Property

        Public ReadOnly Property Copyright As String
            Get
                If (Me.m_Copyright Is Nothing) Then
                    Dim attribute As AssemblyCopyrightAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyCopyrightAttribute)), AssemblyCopyrightAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_Copyright = ""
                    Else
                        Me.m_Copyright = attribute.Copyright
                    End If
                End If
                Return Me.m_Copyright
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                If (Me.m_Description Is Nothing) Then
                    Dim attribute As AssemblyDescriptionAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyDescriptionAttribute)), AssemblyDescriptionAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_Description = ""
                    Else
                        Me.m_Description = attribute.Description
                    End If
                End If
                Return Me.m_Description
            End Get
        End Property

        Public ReadOnly Property DirectoryPath As String
            Get
                Return Path.GetDirectoryName(Me.m_Assembly.Location)
            End Get
        End Property

        Public ReadOnly Property LoadedAssemblies As ReadOnlyCollection(Of Assembly)
            Get
                Dim list As New Collection(Of Assembly)
                Dim assembly As Assembly
                For Each assembly In AppDomain.CurrentDomain.GetAssemblies
                    list.Add([assembly])
                Next
                Return New ReadOnlyCollection(Of Assembly)(list)
            End Get
        End Property

        Public ReadOnly Property ProductName As String
            Get
                If (Me.m_ProductName Is Nothing) Then
                    Dim attribute As AssemblyProductAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyProductAttribute)), AssemblyProductAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_ProductName = ""
                    Else
                        Me.m_ProductName = attribute.Product
                    End If
                End If
                Return Me.m_ProductName
            End Get
        End Property

        Public ReadOnly Property StackTrace As String
            Get
                Return Environment.StackTrace
            End Get
        End Property

        Public ReadOnly Property Title As String
            Get
                If (Me.m_Title Is Nothing) Then
                    Dim attribute As AssemblyTitleAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyTitleAttribute)), AssemblyTitleAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_Title = ""
                    Else
                        Me.m_Title = attribute.Title
                    End If
                End If
                Return Me.m_Title
            End Get
        End Property

        Public ReadOnly Property Trademark As String
            Get
                If (Me.m_Trademark Is Nothing) Then
                    Dim attribute As AssemblyTrademarkAttribute = DirectCast(Me.GetAttribute(GetType(AssemblyTrademarkAttribute)), AssemblyTrademarkAttribute)
                    If (attribute Is Nothing) Then
                        Me.m_Trademark = ""
                    Else
                        Me.m_Trademark = attribute.Trademark
                    End If
                End If
                Return Me.m_Trademark
            End Get
        End Property

        Public ReadOnly Property Version As Version
            Get
                Return Me.m_Assembly.GetName.Version
            End Get
        End Property

        Public ReadOnly Property WorkingSet As Long
            Get
                Return Environment.WorkingSet
            End Get
        End Property


        ' Fields
        Private m_Assembly As Assembly
        Private m_CompanyName As String = Nothing
        Private m_Copyright As String = Nothing
        Private m_Description As String = Nothing
        Private m_ProductName As String = Nothing
        Private m_Title As String = Nothing
        Private m_Trademark As String = Nothing
    End Class
End Namespace

