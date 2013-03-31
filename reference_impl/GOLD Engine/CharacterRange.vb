Imports System.Text

Friend Class CharacterRange
    Public Start As UInt16
    Public [End] As UInt16

    Public Sub New(ByVal Start As UInt16, ByVal [End] As UInt16)
        Me.Start = Start
        Me.End = [End]
    End Sub
End Class

Friend Class CharacterSet
    Inherits ArrayList

    Default Public Shadows Property Item(ByVal Index As Integer) As CharacterRange
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As CharacterRange)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByRef Item As CharacterRange) As Integer
        Return MyBase.Add(Item)
    End Function

    Public Shadows Function Contains(ByVal CharCode As Integer) As Boolean
        'This procedure searchs the set to deterimine if the CharCode is in one
        'of the ranges - and, therefore, the set.
        'The number of ranges in any given set are relatively small - rarely 
        'exceeding 10 total. As a result, a simple linear search is sufficient 
        'rather than a binary search. In fact, a binary search overhead might
        'slow down the search!

        Dim Found As Boolean = False
        Dim n As Integer = 0
        Dim Range As CharacterRange

        Do While (n < MyBase.Count) And (Not Found)
            Range = MyBase.Item(n)

            Found = (CharCode >= Range.Start And CharCode <= Range.End)
            n += 1
        Loop

        Return Found
    End Function

End Class

Friend Class CharacterSetList
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

    Default Public Shadows Property Item(ByVal Index As Integer) As CharacterSet
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As CharacterSet)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByRef Item As CharacterSet) As Integer
        Return MyBase.Add(Item)
    End Function
End Class