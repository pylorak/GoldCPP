Option Strict Off
Option Explicit On

Friend Enum LRConflict
    ShiftShift = 1      'Never happens
    ShiftReduce = 2
    ReduceReduce = 3
    AcceptReduce = 4    'Never happens with this implementation
    None = 5
End Enum

'===== NOTE: MUST MATCH FILE DEFINITION
Friend Enum LRActionType
    Shift = 1 'Shift a symbol and goto a state
    Reduce = 2 'Reduce by a specified rule
    [Goto] = 3 'Goto to a state on reduction
    Accept = 4 'Input successfully parsed
    [Error] = 5 'Programmars see this often!
End Enum

Friend Class LRAction
    Public Symbol As Symbol
    Public Type As LRActionType
    Public Value As Short             'shift to state, reduce rule, goto state

    Public Sub New(ByVal TheSymbol As Symbol, ByVal Type As LRActionType, ByVal Value As Short)
        Me.Symbol = TheSymbol
        Me.Type = Type
        Me.Value = Value
    End Sub
End Class

Friend Class LRState
    Inherits ArrayList

    Public Shadows Function IndexOf(ByVal Item As Symbol) As Short
        'Returns the index of SymbolIndex in the table, -1 if not found
        Dim n, Index As Short
        Dim Found As Boolean

        n = 0
        Found = False
        Do While (Not Found) And n < MyBase.Count
            If Item.Equals(MyBase.Item(n).Symbol) Then
                Index = n
                Found = True
            End If
            n += 1
        Loop

        If Found Then
            Return Index
        Else
            Return -1
        End If
    End Function

    Public Shadows Sub Add(ByVal Action As LRAction)
        MyBase.Add(Action)
    End Sub

    Default Public Shadows Property Item(ByVal Index As Short) As LRAction
        Get
            Return MyBase.Item(Index)
        End Get
        Set(ByVal Value As LRAction)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Default Public Shadows Property Item(ByVal Sym As Symbol) As LRAction
        Get
            Dim Index As Integer = IndexOf(Sym)
            If Index <> -1 Then
                Return MyBase.Item(Index)
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal Value As LRAction)
            Dim Index As Integer = IndexOf(Sym)
            If Index <> -1 Then
                MyBase.Item(Index) = Value
            End If
        End Set
    End Property
End Class

Friend Class LRStateList
    Inherits ArrayList

    Public InitialState As Short

    Public Sub New()
        MyBase.New()
        InitialState = 0
    End Sub

    Friend Sub New(ByVal Size As Integer)
        MyBase.New()
        ReDimension(Size)
        InitialState = 0
    End Sub

    Friend Sub ReDimension(ByVal Size As Integer)
        'Increase the size of the array to Size empty elements.
        Dim n As Integer

        MyBase.Clear()
        For n = 0 To Size - 1
            MyBase.Add(Nothing)
        Next
    End Sub

    Default Public Shadows Property Item(ByVal Index As Integer) As LRState
        Get
            Return MyBase.Item(Index)
        End Get

        Set(ByVal Value As LRState)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByRef Item As LRState) As Integer
        Return MyBase.Add(Item)
    End Function
End Class