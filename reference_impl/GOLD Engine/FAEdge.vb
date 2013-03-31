Option Strict Off
Option Explicit On
Friend Class FAEdge
	'================================================================================
	' Class Name:
    '      FAEdge
    '
	' Purpose:
	'      Each state in the Determinstic Finite Automata contains multiple edges which
	'      link to other states in the automata.
	'
	'      This class is used to represent an edge.
	'
	' Author(s):
	'      Devin Cook
    '      http://www.DevinCook.com/GOLDParser
	'
	' Dependacies:
	'      (None)
	'
	'================================================================================

    Public Characters As CharacterSet         'Characters to advance on	
    Public Target As Integer                  'FAState

    Public Sub New(ByVal CharSet As CharacterSet, ByVal Target As Integer)
        Me.Characters = CharSet
        Me.Target = Target
    End Sub

    Public Sub New()
        'Nothing for now
    End Sub
End Class

Friend Class FAEdgeList
    Inherits ArrayList

    Default Public Shadows Property Item(ByVal Index As Integer) As FAEdge
        Get
            Return MyBase.Item(Index)
        End Get
        Set(ByVal Value As FAEdge)
            MyBase.Item(Index) = Value
        End Set
    End Property

    Public Shadows Function Add(ByVal Edge As FAEdge) As Integer
        Return MyBase.Add(Edge)
    End Function
End Class
