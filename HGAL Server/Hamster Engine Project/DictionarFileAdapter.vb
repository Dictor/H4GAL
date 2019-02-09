Module DictionarFileAdapter
    Public Sub Write(path As String, data As Dictionary(Of String, String))
        Dim xmlDoc = New XDocument(New XDeclaration("1.0", "UTF-8", "YES"))
        Dim xmlRoot = New XElement("DICTIONARY")
        xmlDoc.Add(xmlRoot)
        For Each nowelement As KeyValuePair(Of String, String) In data
            Dim xmlElement As New XElement("KEYVALUEPAIR", New XAttribute("KEY", nowelement.Key),
                                                           New XElement("DATA", nowelement.Value))
            xmlRoot.Add(xmlElement)
        Next
        xmlDoc.Save(path)
    End Sub

    Public Sub Write(path As String, data As Dictionary(Of String, Dictionary(Of String, String)))
        Dim xmlDoc = New XDocument(New XDeclaration("1.0", "UTF-8", "YES"))
        Dim xmlRoot = New XElement("DICTIONARY")
        xmlDoc.Add(xmlRoot)
        For Each nowelement As KeyValuePair(Of String, Dictionary(Of String, String)) In data
            Dim xmlElement As New XElement("KEYVALUEPAIR", New XAttribute("KEY", nowelement.Key))
            For Each nowchildelement As KeyValuePair(Of String, String) In nowelement.Value
                xmlElement.Add(New XElement("KEYVALUEPAIR", New XAttribute("NAME", nowchildelement.Key), New XAttribute("DATA", nowchildelement.Value)))
            Next
            xmlRoot.Add(xmlElement)
        Next
        xmlDoc.Save(path)
    End Sub

    Public Function ReadSingle(path As String) As Dictionary(Of String, String)
        Dim xmlDoc = XDocument.Load(path)
        Dim xmlElements As IEnumerable(Of XElement) = xmlDoc.Root.Elements()
        Dim data As New Dictionary(Of String, String)
        For Each nowelement As XElement In xmlElements
            data.Add(nowelement.Attribute("KEY").Value, nowelement.Element("DATA").Value)
        Next
        Return data
    End Function

    Public Function ReadMulti(path As String) As Dictionary(Of String, Dictionary(Of String, String))
        Dim xmlDoc = XDocument.Load(path)
        Dim xmlElement As IEnumerable(Of XElement) = xmlDoc.Root.Elements()
        Dim data As New Dictionary(Of String, Dictionary(Of String, String))
        For Each nowelement As XElement In xmlElement
            Dim xmlChildElement As IEnumerable(Of XElement) = nowelement.Elements
            Dim childdata As New Dictionary(Of String, String)
            For Each childelement As XElement In xmlChildElement
                childdata.Add(childelement.Attribute("NAME").Value, childelement.Attribute("DATA").Value)
            Next
            data.Add(nowelement.Attribute("KEY").Value, childdata)
        Next
        Return data
    End Function
End Module
