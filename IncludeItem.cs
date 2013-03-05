using EnvDTE;

namespace FileIncluder
{
    public class IncludeItem
    {
        public string Project { get; set; }
        public string ProjectId { get; set; }
        public string File { get; set; }
        public bool Selected { get; set; }
        public string FullPath { get; set; }
    }
}
