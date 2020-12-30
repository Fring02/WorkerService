using MessageQueueApi.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace MessageLoggingService
{
    enum FileChangeType
    {
        Changed, Renamed, Deleted, Created, Error
    }
}
