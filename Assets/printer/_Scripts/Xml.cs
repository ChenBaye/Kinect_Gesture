/*
 * 2018-06-27 ZoJet
*/

using System.IO;
using System.Xml;

public class Xml {
    /// <summary>
    /// Split a string to string array by symbol
    /// </summary>
    /// <param name="_str"></param>
    /// <param name="_symbol"></param>
    /// <returns></returns>
    public static string[] SplitToStringArray(string _str, char _symbol = '/') {
        if (_str == null) {
            return null;
        }
        string[] strs = _str.Split(_symbol);
        return strs;
    }

    /// <summary>
    /// Read an element data form xml by nodes' name
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_nodes"></param>
    /// <returns></returns>
    public static string ReadElement(string _path, string[] _nodes) {
        if (!File.Exists(_path)) {
            return "";
        }

        string str = "";
        XmlDocument doc = new XmlDocument();
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;//忽略注释内容
        XmlReader reader = XmlReader.Create(_path, settings);
        doc.Load(reader);

        int nodeCount = _nodes.Length;
        int index = 1;
        XmlNodeList nodeList = doc.SelectSingleNode(_nodes[0]).ChildNodes;

        if (nodeList == null) {
            return "";
        }

        while (index < nodeCount) {
            bool isChecked = false;
            foreach (XmlNode ele in nodeList) {
                if (ele.Name == _nodes[index]) {
                    isChecked = true;
                    index++;
                    if (index >= nodeCount) {
                        str = ele.InnerText;
                        nodeList = null;
                        break;
                    }
                    nodeList = ele.ChildNodes;
                    break;
                }
            }
            if (!isChecked) {
                break;
            }
        }
        return str;
    }

    /// <summary>
    /// Read elements' data from xml by nodes' name
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_nodes"></param>
    /// <returns></returns>
    public static string[] ReadElements(string _path, string[] _nodes) {
        if (!File.Exists(_path)) {
            return null;
        }

        string[] str = null;
        XmlDocument doc = new XmlDocument();
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        XmlReader reader = XmlReader.Create(_path, settings);
        doc.Load(reader);

        int nodeCount = _nodes.Length;
        int index = 1;
        XmlNodeList nodeList = doc.SelectSingleNode(_nodes[0]).ChildNodes;

        if (nodeList == null) {
            return null;
        }

        while (index < nodeCount) {
            bool isChecked = false;
            foreach (XmlNode ele in nodeList) {
                if (ele.Name == _nodes[index]) {
                    isChecked = true;
                    index++;
                    if (index >= nodeCount) {
                        str = new string[ele.ChildNodes.Count];
                        int childIndex = 0;
                        foreach (XmlNode child in ele.ChildNodes) {
                            str[childIndex] = child.InnerText;
                            childIndex++;
                        }
                        nodeList = null;
                        break;
                    }
                    nodeList = ele.ChildNodes;
                    break;
                }
            }
            if (!isChecked) {
                break;
            }
        }
        return str;
    }

    /// <summary>
    /// Write an element data by xml path , nodes and data
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_xmlNodes"></param>
    /// <param name="_data"></param>
    /// <returns></returns>
    public static bool WriteElement(string _path, string[] _xmlNodes, string _data) {
        XmlDocument doc = new XmlDocument();
        if (File.Exists(_path)) {
            doc.Load(_path);
        }
        int nodeNum = _xmlNodes.Length;
        int counter = 1;
        XmlNodeList nodelist = doc.SelectSingleNode(_xmlNodes[0]).ChildNodes;
        if (nodelist == null)
            return false;
        while (counter < nodeNum) {
            bool checkok = false;
            foreach (XmlNode element in nodelist) {
                if (element.Name == _xmlNodes[counter]) {
                    checkok = true;
                    counter++;
                    if (counter >= nodeNum) {
                        element.InnerText = _data;
                        doc.Save(_path);
                        nodelist = null;
                        return true;
                    }
                    nodelist = element.ChildNodes;
                    break;
                }
            }
            if (!checkok) return false;
        }
        return false;
    }

    /// <summary>
    /// Write elements data by xml path , nodes and data
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_xmlNodes"></param>
    /// <param name="_elementName"></param>
    /// <param name="_data"></param>
    public static void WriteElements(string _path, string[] _xmlNodes, string _elementName, string _data) {
        XmlDocument doc = new XmlDocument();
        if (File.Exists(_path)) {
            doc.Load(_path);
        }
        int nodeNum = _xmlNodes.Length;
        int counter = 1;
        XmlNodeList nodelist = doc.SelectSingleNode(_xmlNodes[0]).ChildNodes;
        if (nodelist == null)
            return;
        while (counter < nodeNum) {
            bool checkok = false;
            foreach (XmlNode node in nodelist) {
                if (node.Name == _xmlNodes[counter]) {
                    checkok = true;
                    counter++;
                    if (counter >= nodeNum) {
                        XmlElement Aelement = doc.CreateElement(_elementName);
                        Aelement.InnerText = _data;
                        node.AppendChild(Aelement);
                        doc.Save(_path);
                        nodelist = null;
                        return;
                    }
                    nodelist = node.ChildNodes;
                    break;
                }
            }
            if (!checkok) break;
        }
    }
}


