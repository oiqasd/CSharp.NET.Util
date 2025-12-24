using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CSharp.Net.Update
{
    internal class XmlParse
    {
        /// <summary>
        /// 使用 XmlDocument 解析 XML 字符串
        /// </summary>
        /// <param name="xml"></param>
        public static void ParseByXmlDocument(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            // 获取所有Person节点
            XmlNodeList personNodes = doc.SelectNodes("//Person");

            foreach (XmlNode personNode in personNodes)
            {
                string id = personNode.Attributes["id"].Value;
                string name = personNode["Name"].InnerText;
                string age = personNode["Age"].InnerText;
                string email = personNode["Email"].InnerText;
            }
        }
        // 使用LINQ to XML解析XML
        static void ParseWithLinqToXml(string xml)
        {
            XDocument doc = XDocument.Parse(xml);

            // 查询所有Person节点
            var persons = from person in doc.Descendants("Person")
                          select new
                          {
                              Id = person.Attribute("id").Value,
                              Name = person.Element("Name").Value,
                              Age = person.Element("Age").Value,
                              Email = person.Element("Email").Value
                          };

            foreach (var person in persons)
            {
                Console.WriteLine($"ID: {person.Id}, 姓名: {person.Name}, 年龄: {person.Age}, 邮箱: {person.Email}");
            }
        }

        // 使用XPath解析XML
        static void ParseWithXPath(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            // 使用XPath选择所有Person节点
            XmlNodeList personNodes = doc.SelectNodes("/Root/Person");

            foreach (XmlNode personNode in personNodes)
            {
                string id = personNode.Attributes["id"].Value;
                string name = personNode.SelectSingleNode("Name").InnerText;
                string age = personNode.SelectSingleNode("Age").InnerText;
                string email = personNode.SelectSingleNode("Email").InnerText;

                Console.WriteLine($"ID: {id}, 姓名: {name}, 年龄: {age}, 邮箱: {email}");
            }
        }
    }
}
