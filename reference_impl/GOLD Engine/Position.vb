Public Class Position
    Public Line As Integer
    Public Column As Integer

    Friend Sub New()
        Me.Line = 0
        Me.Column = 0
    End Sub

    Friend Sub Copy(ByVal Pos As Position)
        Me.Column = Pos.Column
        Me.Line = Pos.Line
    End Sub
End Class
