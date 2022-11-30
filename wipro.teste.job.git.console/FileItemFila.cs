using FileHelpers;

namespace wipro.teste.job.console
{
    [DelimitedRecord(";")]
    internal class FileItemFila
    {
        public string IdMoeda { get; set; }

        public string DatCotacao { get; set; }

        public float VlCotacao { get; set; }
    }
}
