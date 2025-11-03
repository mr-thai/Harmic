namespace Harmic.Areas.Admin.Models
{
    public class SummerNote
    {
        public SummerNote(string idEditor, bool loadLibrary = true)
        {
            IdEditor = idEditor;
            LoadLibrary = loadLibrary;
        }
        public string IdEditor { get; set; }
        public bool LoadLibrary { get; set; }
        public string Height { get; set; } = "500";
        public string toolbar { get; set; } = @"[
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['fontname', ['fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', elfinderFiles , 'picture', 'video']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ]";
    }
}