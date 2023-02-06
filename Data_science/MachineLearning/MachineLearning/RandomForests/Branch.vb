Imports stdNum = System.Math

Namespace RandomForests

    Public Class Branch

        Friend mean, mean_snp As Double
        Friend class_val As Integer
        ''' <summary>
        ''' 'F' for final branch
        ''' </summary>
        Friend status As String = " "
        Friend Feature, Child1, Child2, Parent As Integer
        Friend list As New List(Of Integer)()

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMean(phen As Double()) As Double

            Dim i = 0
            mean = 0.0R
            For i = 0 To list.Count - 1
                mean = mean + phen(list(i))
            Next
            mean = mean / list.Count
            Return mean
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getClass(phen As Double()) As Integer
            Dim i = 0
            Dim temp = New Integer(2) {}
            For i = 0 To list.Count - 1
                temp(phen(list(i))) += 1
            Next
            If temp(0) > temp(1) And temp(0) > temp(2) Then
                class_val = 0
            ElseIf temp(1) > temp(2) Then
                class_val = 1
            Else
                class_val = 2
            End If
            Return class_val
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMSE(phen As Double()) As Double
            Dim i = 0
            getMean(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + (phen(list(i)) - mean) * (phen(list(i)) - mean)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function

        ''' <summary>
        ''' This method returns the SNP for a given position.
        '''  It needs as arguments:
        '''  @arg position, the position of the SNP in the genomic combination
        ''' </summary>
        Public Overridable Function getMissClass(phen As Double()) As Double
            Dim i = 0
            getClass(phen)
            Dim MSE = 0.0R
            For i = 0 To list.Count - 1
                MSE = MSE + stdNum.Abs(CInt(phen(list(i))) - class_val)
            Next
            'MSE=MSE/list.size();
            Return MSE
        End Function
    End Class
End Namespace