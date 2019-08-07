using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePoint.Files.SearchConnector
{
    public class SearchFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public String ContentType { get; set; }
        public DateTime LastModified { get; set; }
        public string docaclmeta { get; set; }
        public Boolean UsesPluggableAuth { get; set; }
        public Byte[] SecurityDescriptor { get; set; }
    }


   // CreatedOn = aFile.TimeCreated,
   // ModifiedOn = aFile.TimeLastModified,
   // Extension = ext,
   // ContentType = mim,
   // SPContentType = "Document",
   // ModifiedBy = aFile.Author.Title,
    public class SearchFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime LastModified { get; set; }
        public Byte[] SecurityDescriptor { get; set; }
        public string docaclmeta { get; set; }
        public Boolean UsesPluggableAuth { get; set; }
    }
}
