using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace MVCForum.Utilities
{
    [Obsolete("在爱驴网应用中无需要使用。")]
    public class RssReader
    {
        public List<RssItem> GetRssFeed(string url)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.UserAgent = "Fiddler";

                var rep = req.GetResponse();
                var reader = XmlReader.Create(rep.GetResponseStream());
                var doc = XDocument.Load(reader, LoadOptions.None);

                return (from i in doc.Descendants("channel").Elements("item")
                        select new RssItem
                        {
                            Title = i.Element("title").Value,
                            Link = i.Element("link").Value,
                            Description = i.Element("description").Value
                        }).ToList();
            }
            catch (Exception ex)
            {
                string errormsg = ex.Message;
                //log the error message
                return null;
            }
        }
    }

    [Obsolete("在爱驴网应用中无需要使用。")]
    [Serializable]
    public class RssItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
    }
}
