Option Strict Off
Option Explicit On

Imports System.ComponentModel

Public Class Token
    '================================================================================
    ' Class Name:
    '      Token
    '
    ' Purpose:
    '       While the Symbol represents a class of terminals and nonterminals, the
    '       Token represents an individual piece of information.
    '       Ideally, the token would inherit directly from the Symbol Class, but do to
    '       the fact that Visual Basic 5/6 does not support this aspect of Object Oriented
    '       Programming, a Symbol is created as a member and its methods are mimicked.
    '
    ' Author(s):
    '      Devin Cook
    '
    ' Dependacies:
    '      Symbol, Position
    '
    '================================================================================
    Private m_State As Short
    Private m_Data As Object
    Private m_Parent As Symbol
    Private m_Position As New Position

    Friend Sub New()
        m_Parent = Nothing
        m_Data = Nothing
        m_State = 0
    End Sub

    Public Sub New(ByVal Parent As Symbol, ByVal Data As Object)
        m_Parent = Parent
        m_Data = Data
        m_State = 0
    End Sub

    <Description("Returns the line/column position where the token was read.")> _
    Public Function Position() As Position
        Return m_Position
    End Function

    <Description("Returns/sets the object associated with the token.")> _
    Public Property Data() As Object
        Get
            Return m_Data
        End Get
        Set(ByVal Value As Object)
            m_Data = Value
        End Set
    End Property

    Friend Property State() As Short
        Get
            Return m_State
        End Get
        Set(ByVal Value As Short)
            m_State = Value
        End Set
    End Property

    <Description("Returns the parent symbol of the token.")> _
    Public Property Parent() As Symbol
        Get
            Return m_Parent
        End Get
        Friend Set(ByVal Value As Symbol)
            m_Parent = Value
        End Set
    End Property

    <Description("Returns the symbol type associated with this token.")> _
    Public Function Type() As SymbolType
        Return m_Parent.Type
    End Function

    Friend Function Group() As Group
        Return m_Parent.Group
    End Function
End Class

Public Class TokenList
    Private m_Array As ArrayList   'Don't inherit - hide array modifying methods

    Friend Sub New()
        m_Array = New ArrayList
    End Sub

    <Description("Returns the token with the specified index.")> _
    Default Public Shadows Property Item(ByVal Index As Integer) As Token
        Get
            Return m_Array(Index)
        End Get

        Friend Set(ByVal Value As Token)
            m_Array(Index) = Value
        End Set
    End Property

    Friend Function Add(ByVal Item As Token) As Integer
        Return m_Array.Add(Item)
    End Function

    <Description("Returns the total number of tokens in the list.")> _
    Public Function Count() As Integer
        Return m_Array.Count
    End Function

    Friend Sub Clear()
        m_Array.Clear()
    End Sub
End Class

Friend Class TokenStack
    '================================================================================
    ' Class Name:
    '      TokenStack    '
    ' Instancing:
    '      Private; Internal  (VB Setting: 1 - Private)
    '
    ' Purpose:
    '      This class is used by the GOLDParser class to store tokens during parsing.
    '      In particular, this class is used the the LALR(1) state machine.
    '
    ' Author(s):
    '      Devin Cook
    '      GOLDParser@DevinCook.com
    '
    ' Dependacies:
    '      Token Class
    '
    ' Revision History
    '     12/11/2001
    '         Modified the stack to not deallocate the array until cleared
    '================================================================================

    Private m_Stack As Stack

    Public Sub New()
        m_Stack = New Stack
    End Sub

    Friend ReadOnly Property Count() As Integer
        Get
            Return m_Stack.Count
        End Get
    End Property

    Public Sub Clear()
        m_stack.Clear()
    End Sub

    Public Sub Push(ByRef TheToken As Token)
        m_Stack.Push(TheToken)
    End Sub

    Public Function Pop() As Token
        Return m_Stack.Pop()
    End Function

    Public Function Top() As Token
        Return m_Stack.Peek()
    End Function
End Class

Friend Class TokenQueueStack
    Private m_Items As ArrayList

    Public Sub New()
        m_Items = New ArrayList
    End Sub

    Friend ReadOnly Property Count() As Integer
        Get
            Return m_Items.Count
        End Get
    End Property

    Public Sub Clear()
        m_Items.Clear()
    End Sub

    Public Sub Enqueue(ByRef TheToken As Token)
        m_Items.Add(TheToken)    'End of list
    End Sub

    Public Function Dequeue() As Token
        Dim Result As Token
        Result = m_Items(0) 'Front of list
        m_Items.RemoveAt(0)

        Return Result
    End Function

    Public Function Top() As Token
        If m_Items.Count >= 1 Then
            Return m_Items(0)
        Else
            Return Nothing
        End If
    End Function

    Public Sub Push(ByVal TheToken As Token)
        m_Items.Insert(0, TheToken)
    End Sub
    Public Function Pop() As Token
        Return Dequeue()                  'Same as dequeue
    End Function
End Class
