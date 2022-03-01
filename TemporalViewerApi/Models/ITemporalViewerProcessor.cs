using System.Collections.Generic;

namespace TemporalViewerApi.Models
{
    public interface ITemporalViewerProcessor
    {
        List<string> Messages { get; set; }
        TemporalViewerResults Process(TemporalViewerRequest lookupKeys);
    }
}
