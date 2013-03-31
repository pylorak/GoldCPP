Imports System.Text
Imports System.IO

Public Class frmMain

    Private Sub DrawReductionTree(ByVal Root As GOLD.Reduction)
        'This procedure starts the recursion that draws the parse tree.
        Dim Tree As New StringBuilder

        Tree.AppendLine("+-" & Root.Parent.Text)
        DrawReduction(Tree, Root, 1)

        txtParseTree.Text = Tree.ToString
    End Sub

    Private Sub DrawReduction(ByRef Tree As StringBuilder, ByVal Reduction As GOLD.Reduction, ByVal Indent As Integer)
        'This is a simple recursive procedure that draws an ASCII version of the parse
        'tree

        Dim n As Integer, IndentText As String = ""

        For n = 1 To Indent
            IndentText &= "| "
        Next

        '=== Display the children of the reduction
        For n = 0 To Reduction.Count - 1
            Select Case Reduction(n).Type
                Case GOLD.SymbolType.Nonterminal
                    Tree.AppendLine(IndentText & "+-" & Reduction(n).Data.Parent.Text)
                    DrawReduction(Tree, Reduction(n).Data, Indent + 1)
                Case Else
                    Tree.AppendLine(IndentText & "+-" & Reduction(n).Data)
            End Select
        Next
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        btnLoad.Enabled = True
        btnParse.Enabled = False

        txtTableFile.Text = Application.StartupPath & "\simple 2.egt"
    End Sub

    Private Sub btnParse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnParse.Click
        btnParse.Enabled = False

        If MyParser.Parse(New StringReader(txtSource.Text)) Then
            DrawReductionTree(MyParser.Root)
        Else
            txtParseTree.Text = MyParser.FailMessage
        End If

        btnParse.Enabled = True
    End Sub

    Private Sub btnLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLoad.Click
        'This procedure can be called to load the parse tables. The class can
        'read tables using a BinaryReader.
        Try
            If MyParser.Setup(txtTableFile.Text) Then
                'Change button enable/disable for the user
                btnLoad.Enabled = False
                btnParse.Enabled = True
            Else
                MsgBox("CGT failed to load")
            End If

        Catch ex As GOLD.ParserException
            MsgBox(ex.Message)
        End Try

    End Sub
End Class
