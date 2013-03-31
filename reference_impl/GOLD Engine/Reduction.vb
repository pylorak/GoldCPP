Option Strict Off
Option Explicit On
Imports System.ComponentModel

Public Class Reduction
    '================================================================================
    ' Class Name:
    '      Reduction
    '
    ' Instancing:
    '      Public; Creatable  (VB Setting: 2 - PublicNotCreatable)
    '
    ' Purpose:
    '      This class is used by the engine to hold a reduced rule. Rather the contain
    '      a list of Symbols, a reduction contains a list of Tokens corresponding to the
    '      the rule it represents. This class is important since it is used to store the
    '      actual source program parsed by the Engine.
    '
    ' Author(s):
    '      Devin Cook
    '
    ' Dependacies:
    '================================================================================

    Inherits TokenList

    Private m_Parent As Production
    Private m_Tag As Object

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

    <Description("Returns the parent production.")> _
    Public Property Parent() As Production
        Get
            Return m_Parent
        End Get
        Friend Set(ByVal Value As Production)
            m_Parent = Value
        End Set
    End Property

    <Description("Returns/sets any additional user-defined data to this object.")> _
    Public Property Tag() As Object
        Get
            Return m_Tag
        End Get
        Set(ByVal Value As Object)
            m_Tag = Value
        End Set
    End Property

    <Description("Returns/sets the parse data stored in the token. It is a shortcut to Item(Index).Token.")> _
    Public Property Data(ByVal Index As Integer) As Object
        Get
            Return MyBase.Item(Index).Data
        End Get
        Set(ByVal value As Object)
            MyBase.Item(Index).Data = value
        End Set
    End Property
End Class