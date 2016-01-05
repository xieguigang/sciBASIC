''' Author:  Matthew Johnson
''' Version: 1.0
''' Date:    March 13, 2006
''' Notice:  You are free to use this code as you wish.  There are no guarantees whatsoever about
''' its usability or fitness of purpose.

#Region "using"
#End Region

Namespace Nexus.Reflection
	Public Class ProxyException
		Inherits Exception
	End Class

	Public Class ProxyAttributeReflectionException
		Inherits ProxyException
	End Class

	Public Class MissingProxyTargetException
		Inherits ProxyException
	End Class
End Namespace
