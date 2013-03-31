

Friend Class Group
    Public Enum AdvanceMode
        Token = 0
        Character = 1
    End Enum

    Public Enum EndingMode
        Open = 0
        Closed = 1
    End Enum

    Friend TableIndex As Short

    Friend Name As String
    Friend Container As Symbol
    Friend Start As Symbol
    Friend [End] As Symbol

    Friend Advance As AdvanceMode
    Friend Ending As EndingMode

    Friend Nesting As IntegerList

    Friend Sub New()
        Advance = AdvanceMode.Character
        Ending = EndingMode.Closed
        Nesting = New IntegerList     'GroupList
    End Sub
End Class


Friend Class GroupList
    Inherits ArrayList

    Public Sub New()
        MyBase.New()
    End Sub

    Friend Sub New(ByVal Size As Integer)
        MyBase.New()
        ReDimension(Size)
    End Sub

    Friend Sub ReDimension(ByVal Size As Integer)
        'Increase the size of the array to Size empty elements.
        Dim n As Integer

        MyBase.Clear()
        For n = 0 To Size - 1
            MyBase.Add(Nothing)
        Next
    End Sub

    Default Public Shadows Property Item(ByVal Index As Integer) As Group
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As Group)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByVal Item As Group) As Integer
        Return MyBase.Add(Item)
    End Function
End Class


Friend Class IntegerList
    Inherits ArrayList
    Default Public Shadows Property Item(ByVal Index As Integer) As Integer
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As Integer)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByVal Value As Integer) As Integer
        Return MyBase.Add(Value)
    End Function

    Public Shadows Function Contains(ByVal Item As Integer) As Boolean
        Return MyBase.Contains(Item)
    End Function
End Class
