Option Strict Off
Option Explicit On
Friend Class FAState
    '================================================================================
    ' Class Name:
    '      FAState
    '
    ' Purpose:
    '      Represents a state in the Deterministic Finite Automata which is used by
    '      the tokenizer.
    '
    ' Author(s):
    '      Devin Cook
    '
    ' Dependacies:
    '      FAEdge, Symbol
    '
    '================================================================================

    Public Edges As FAEdgeList
    Public Accept As Symbol

    Public Sub New(ByVal Accept As Symbol)
        Me.Accept = Accept
        Me.Edges = New FAEdgeList
    End Sub

    Public Sub New()
        Me.Accept = Nothing
        Me.Edges = New FAEdgeList
    End Sub
End Class

Friend Class FAStateList
    Inherits ArrayList

    Public InitialState As Short

    '===== DFA runtime variables
    Public ErrorSymbol As Symbol

    Public Sub New()
        MyBase.New()

        InitialState = 0
        ErrorSymbol = Nothing
    End Sub

    Friend Sub New(ByVal Size As Integer)
        MyBase.New()
        ReDimension(Size)

        InitialState = 0
        ErrorSymbol = Nothing
    End Sub

    Friend Sub ReDimension(ByVal Size As Integer)
        'Increase the size of the array to Size empty elements.
        Dim n As Integer

        MyBase.Clear()
        For n = 0 To Size - 1
            MyBase.Add(Nothing)
        Next
    End Sub

    Default Public Shadows Property Item(ByVal Index As Integer) As FAState
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As FAState)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByVal Item As FAState) As Integer
        Return MyBase.Add(Item)
    End Function
End Class