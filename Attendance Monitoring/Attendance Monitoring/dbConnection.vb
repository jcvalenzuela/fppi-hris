Imports System.Data.SqlClient
Module dbConnection
    Public conString As String = "SERVER=DESKTOP-FB1NJLP\SQLEXPRESS,49170;Initial Catalog=FPPI-HRIS;Integrated Security=False;Connect Timeout=30;User Instance=False;User ID=fs;Password=password123"
    Public conn As SqlConnection
    Public cmd As SqlCommand
    Public ds As New DataSet
    Public adpt As New SqlDataAdapter
    Public tbl As DataTableCollection
    Public sSQL As String
    Public dt As New DataTable
    Public dr As SqlDataReader
    Public numrows As Integer
    Public sqlqry As String
    Public cnn As New SqlConnection(conString)


    Public Sub Connect2DB()
        conn = New SqlConnection(conString)
        conn.Open()
    End Sub
End Module
