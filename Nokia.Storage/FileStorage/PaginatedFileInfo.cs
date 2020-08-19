using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nokia.Storage.FileStorage
{
    public class PaginatedFileInfo
    {
        public IEnumerable<StoredFileInfo> Files { get; set; }
        public long TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
