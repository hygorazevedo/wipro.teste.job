using FileHelpers;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Data;

namespace wipro.teste.job.console
{
    internal class Orquestrador : IOrquestrador
    {
        private readonly HttpClient _client;
        public Orquestrador(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("api");
        }
        public async Task ProcessarTarefa()
        {
            var itemFila = await ObterItemFila();
            var moedas = await LerArquivoAsync("DadosMoeda.csv");
            var cotacoes = await LerArquivoAsync("DadosCotacao.csv");

            if (itemFila is null) return;

            var moedasFiltradas = moedas.Select($"DATA_REF >= '{itemFila.DataInicio.ToString("yyyy-MM-dd")}' AND DATA_REF < '{itemFila.DataFim.ToString("yyyy-MM-dd")}'");


            moedasFiltradas.ToList().ForEach(moeda =>
            {
                var idMoeda = moeda.Field<string>("ID_MOEDA");
                var dataReferencia = moeda.Field<string>("DATA_REF");

                var cotacoesFiltradas = cotacoes.Select($"cod_cotacao = '{(int) Enum.Parse(typeof(CotacaoEnum), moeda.ItemArray[0].ToString())}'");
                var listaCotacoesInserir = new List<FileItemFila>();
                cotacoesFiltradas.ToList().ForEach(cotacao =>
                {
                    listaCotacoesInserir.Add(new FileItemFila
                    {
                        IdMoeda = idMoeda,
                        DatCotacao = DateTime.Parse(cotacao.Field<string>("dat_cotacao")).ToString("yyyy-MM-dd"),
                        VlCotacao = float.Parse(cotacao.Field<string>("vlr_cotacao"))
                    });
                });


                EscreverArquivoCotacaoMoeda(listaCotacoesInserir, DateTime.Parse(dataReferencia));
            });
        }

        public async Task<DataTable> LerArquivoAsync(string path)
        {
            DataTable csvData = new DataTable();

            using (TextFieldParser csvReader = new TextFieldParser(path))
            {
                csvReader.SetDelimiters(new string[] { ";" });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();
                foreach (string column in colFields)
                {
                    DataColumn datecolumn = new DataColumn(column);
                    datecolumn.AllowDBNull = true;
                    csvData.Columns.Add(datecolumn);
                }

                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    csvData.Rows.Add(fieldData);
                }
            }

            return await Task.FromResult(csvData);
        }

        private async Task<ItemFila> ObterItemFila()
        {
            var itemFila = null as ItemFila;
            var result = await _client.GetAsync("https://localhost:7034/api/Item");

            if (result.IsSuccessStatusCode)
            {
                itemFila = JsonConvert.DeserializeObject<ItemFila>(result.Content.ReadAsStringAsync().Result);
            }

            return await Task.FromResult(itemFila);
        }

        private void EscreverArquivoCotacaoMoeda(List<FileItemFila> data, DateTime dataMoeda)
        {
            var fieldHeaders = new List<string> { "ID_MOEDA", "DAT_COTACAO", "VL_COTACAO" };
            FileHelperEngine engine = new FileHelperEngine(typeof(FileItemFila));
            engine.HeaderText = String.Join(";", fieldHeaders);
            engine.WriteFile($"Resultado_{dataMoeda.ToString("yyyyMMdd_hhmmss")}.csv", data);
        }
    }
}
