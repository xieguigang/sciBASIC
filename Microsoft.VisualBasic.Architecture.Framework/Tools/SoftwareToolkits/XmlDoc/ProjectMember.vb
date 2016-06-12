' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Xml

''' <summary>
''' Base class for a method or property.
''' </summary>
Public Class ProjectMember
	Private m_name As [String]
	Private projectType As ProjectType
	Private m_summary As [String]
	Private m_returns As [String]

	Public Property Name() As [String]
		Get
			Return Me.m_name
		End Get

		Set
			Me.m_name = value
		End Set
	End Property

	Public Property Summary() As [String]
		Get
			Return Me.m_summary
		End Get

		Set
			Me.m_summary = value
		End Set
	End Property
	Public Property Returns() As [String]
		Get
			Return Me.m_returns
		End Get

		Set
			Me.m_returns = value
		End Set
	End Property

	Public ReadOnly Property Type() As ProjectType
		Get
			Return Me.projectType
		End Get
	End Property

	Public Sub New(projectType As ProjectType)
		Me.projectType = projectType
	End Sub

	Public Sub LoadFromNode(xn As XmlNode)
		Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

		If summaryNode IsNot Nothing Then
			Me.m_summary = summaryNode.InnerText
		End If

		Dim returnsNode As XmlNode = xn.SelectSingleNode("returns")

		If returnsNode IsNot Nothing Then
			Me.m_returns = returnsNode.InnerText
		End If
	End Sub
End Class
