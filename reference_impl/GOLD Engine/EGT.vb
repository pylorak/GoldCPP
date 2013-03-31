Option Strict Off
Option Explicit On
Option Compare Text

Imports System.IO
Imports System.Text


Friend Enum EGTRecord As Byte
    InitialStates = 73  'I
    Symbol = 83         'S
    Production = 82     'R   R for Rule (related productions)
    DFAState = 68       'D
    LRState = 76        'L
    [Property] = 112    'p
    CharRanges = 99     'c 
    Group = 103         'g
    TableCounts = 116   't   Table Counts
End Enum

Friend Class EGTReader
    Public Enum EntryType As Byte
        Empty = 69      'E
        UInt16 = 73     'I - Unsigned, 2 byte
        [String] = 83   'S - Unicode format
        [Boolean] = 66  'B - 1 Byte, Value is 0 or 1
        [Byte] = 98     'b
        [Error] = 0
    End Enum


    Public Class IOException
        Inherits System.Exception

        Public Sub New(ByVal Message As String, ByVal Inner As System.Exception)
            MyBase.New(Message, Inner)
        End Sub

        Public Sub New(ByVal Type As EntryType, ByVal Reader As BinaryReader)
            MyBase.new("Type mismatch in file. Read '" & ChrW(Type) & "' at " & Reader.BaseStream.Position)
        End Sub
    End Class

    Public Class Entry
        Public Type As EntryType
        Public Value As Object

        Public Sub New()
            Type = EntryType.Empty
            Value = ""
        End Sub

        Public Sub New(ByVal Type As EntryType, ByVal Value As Object)
            Me.Type = Type
            Me.Value = Value
        End Sub
    End Class

    Private Const kRecordContentMulti As Byte = 77 'M
    Private m_FileHeader As String
    Private m_Reader As BinaryReader

    'Current record 
    Private m_EntryCount As Integer
    Private m_EntriesRead As Integer

    Public Function RecordComplete() As Boolean
        Return m_EntriesRead >= m_EntryCount
    End Function

    Public Sub Close()
        If Not m_Reader Is Nothing Then
            m_Reader.Close()
            m_Reader = Nothing
        End If
    End Sub

    Public Function EntryCount() As Short
        Return m_EntryCount
    End Function

    Public Function EndOfFile() As Boolean
        Return m_Reader.BaseStream.Position = m_Reader.BaseStream.Length
    End Function

    Public Function Header() As String
        Return m_FileHeader
    End Function

    Public Sub Open(ByVal Reader As BinaryReader)
        m_Reader = Reader

        m_EntryCount = 0
        m_EntriesRead = 0
        m_FileHeader = RawReadCString()
    End Sub

    Public Sub Open(ByVal Path As String)
        Open(New BinaryReader(File.Open(Path, FileMode.Open, FileAccess.Read)))
    End Sub

    Public Function RetrieveEntry() As Entry
        Dim Type As Byte
        Dim Result As New Entry

        If RecordComplete() Then
            Result.Type = EntryType.Empty
            Result.Value = ""
        Else
            m_EntriesRead += 1
            Type = m_Reader.ReadByte()          'Entry Type Prefix
            Result.Type = Type

            Select Case Type
                Case EntryType.Empty
                    Result.Value = ""

                Case EntryType.Boolean
                    Dim b As Byte

                    b = m_Reader.ReadByte
                    Result.Value = (b = 1)

                Case EntryType.UInt16
                    Result.Value = RawReadUInt16()

                Case EntryType.String
                    Result.Value = RawReadCString()

                Case EntryType.Byte
                    Result.Value = m_Reader.ReadByte

                Case Else
                    Result.Type = EntryType.Error
                    Result.Value = ""
            End Select
        End If

        Return Result
    End Function

    Private Function RawReadUInt16() As UInt16
        'Read a uint in little endian. This is the format already used
        'by the .NET BinaryReader. However, it is good to specificially
        'define this given byte order can change depending on platform.

        Dim b0, b1 As Integer
        Dim Result As UInt16

        b0 = m_Reader.ReadByte()    'Least significant byte first
        b1 = m_Reader.ReadByte()

        Result = (b1 << 8) + b0

        Return Result
    End Function

    Private Function RawReadCString() As String
        Dim Char16 As UInt16, Text As String = ""
        Dim Done As Boolean = False

        Do Until Done
            Char16 = RawReadUInt16()
            If Char16 = 0 Then
                Done = True
            Else
                Text &= ChrW(Char16)
            End If
        Loop

        Return Text
    End Function


    Public Function RetrieveString() As String
        Dim e As Entry

        e = RetrieveEntry()
        If e.Type = EntryType.String Then
            Return e.Value
        Else
            Throw New IOException(e.Type, m_Reader)
        End If
    End Function

    Public Function RetrieveInt16() As Integer
        Dim e As Entry

        e = RetrieveEntry()
        If e.Type = EntryType.UInt16 Then
            Return e.Value
        Else
            Throw New IOException(e.Type, m_Reader)
        End If
    End Function

    Public Function RetrieveBoolean() As Boolean
        Dim e As Entry

        e = RetrieveEntry()
        If e.Type = EntryType.Boolean Then
            Return e.Value
        Else
            Throw New IOException(e.Type, m_Reader)
        End If
    End Function

    Public Function RetrieveByte() As Byte
        Dim e As Entry

        e = RetrieveEntry()
        If e.Type = EntryType.Byte Then
            Return e.Value
        Else
            Throw New IOException(e.Type, m_Reader)
        End If
    End Function

    Public Function GetNextRecord() As Boolean
        Dim ID As Byte
        Dim Success As Boolean

        '==== Finish current record
        Do While m_EntriesRead < m_EntryCount
            RetrieveEntry()
        Loop

        '==== Start next record
        ID = m_Reader.ReadByte()

        If ID = kRecordContentMulti Then
            m_EntryCount = RawReadUInt16()
            m_EntriesRead = 0
            Success = True
        Else
            Success = False
        End If

        Return Success
    End Function

    Protected Overrides Sub Finalize()
        Close()
    End Sub
End Class

