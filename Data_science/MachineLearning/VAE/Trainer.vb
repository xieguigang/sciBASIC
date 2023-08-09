Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

' // MNIST_DATA_LOCATION set by MNIST cmake config
' std::cout << "MNIST data directory: " << MNIST_DATA_LOCATION << std::endl;

' // Load MNIST data
' mnist::MNIST_dataset<std::vector, std::vector<uint8_t>, uint8_t> dataset =
'     mnist::read_dataset<std::vector, std::vector, uint8_t, uint8_t>(MNIST_DATA_LOCATION);
' // std::cout << dataset.test_images[10000].size() << std::endl;
' Trainer<double, 28, 28> * mnist_vae_trainer = new Trainer<double, 28, 28>(); 
' mnist_vae_trainer->train_vae(1000, dataset.training_images);
' std::cout << "Nbr of training images = " << dataset.training_images.size() << std::endl;
' std::cout << "Nbr of training labels = " << dataset.training_labels.size() << std::endl;
' std::cout << "Nbr of test images = " << dataset.test_images.size() << std::endl;
' std::cout << "Nbr of test labels = " << dataset.test_labels.size() << std::endl;

Public Class Trainer

    Dim loss_vector As Vector = Nothing
    Dim m_vae As Vae

    Public ReadOnly Property Vae As Vae
        Get
            Return m_vae
        End Get
    End Property

    ''' <summary>
    ''' Create a new VAE trainer module code
    ''' </summary>
    ''' <param name="N1">image width</param>
    ''' <param name="N2">image height</param>
    Sub New(N1 As Integer, N2 As Integer)
        m_vae = New Vae(N1, N2)
    End Sub

    Public Sub train_vae(steps As Integer, dataset_input As Double()())
        Dim input As Double() = New Double(dataset_input(0).Length - 1) {}
        Dim div As Double = 1 / 256

        For i As Integer = 0 To std.Min(dataset_input.Length, steps) - 1
            ' handle input
            If dataset_input(i).Length = 0 Then
                Continue For
            End If

            Dim mean As Double = 0
            Dim v As Double() = dataset_input(i)

            For j As Integer = 0 To v.Length - 1
                mean += v(j)
                input(j) = v(j)
            Next

            mean /= v.Length

            For j As Integer = 0 To input.Length - 1
                input(j) -= mean
                input(j) *= div
            Next

            ' update weights
            m_vae.encode(input)
            m_vae.decode()
            m_vae.update(input)
        Next
    End Sub
End Class
